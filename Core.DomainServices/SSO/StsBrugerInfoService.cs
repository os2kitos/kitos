using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.DomainModel.Result;
using Infrastructure.Soap.STSBruger;
using Serilog;

namespace Core.DomainServices.SSO
{
    public class StsBrugerInfoService : IStsBrugerInfoService
    {
        private readonly ILogger _logger;
        private const string EmailTypeIdentifier = StsOrganisationConstants.UserProperties.Email;

        private readonly string _urlServicePlatformBrugerService;
        private readonly string _urlServicePlatformAdresseService;
        private readonly string _urlServicePlatformOrganisationService;
        private readonly string _urlServicePlatformVirksomhedService;
        private readonly string _urlServicePlatformPersonService;
        private readonly string _certificateThumbprint;
        private readonly string _authorizedMunicipalityCvr;

        public StsBrugerInfoService(StsOrganisationIntegrationConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _urlServicePlatformBrugerService = $"https://{configuration.EndpointHost}/service/Organisation/Bruger/5";
            _urlServicePlatformAdresseService = $"https://{configuration.EndpointHost}/service/Organisation/Adresse/5";
            _urlServicePlatformOrganisationService = $"https://{configuration.EndpointHost}/service/Organisation/Organisation/5";
            _urlServicePlatformVirksomhedService = $"https://{configuration.EndpointHost}/service/Organisation/Virksomhed/5";
            _urlServicePlatformPersonService = $"https://{configuration.EndpointHost}/service/Organisation/Person/5";
            _authorizedMunicipalityCvr = configuration.AuthorizedMunicipalityCvr;
        }

        public Maybe<StsBrugerInfo> GetStsBrugerInfo(Guid uuid)
        {
            var brugerInfo = CollectStsBrugerInformationFromUuid(uuid);
            if (brugerInfo.Failed)
            {
                _logger.Error("Failed to resolve UUIDS '{error}'", brugerInfo.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            var (emailAdresseUuid, organisationUuid, personUuid) = brugerInfo.Value;
            var emailsResult = GetStsAdresseEmailFromUuid(emailAdresseUuid);
            if (emailsResult.Failed)
            {
                _logger.Error("Failed to resolve Emails '{error}'", emailsResult.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            var personData = GetStsPersonFromUuid(personUuid);
            if (personData.Failed)
            {
                _logger.Error("Failed to resolve Person '{error}'", personData.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            return
                GetStsVirksomhedFromUuid(organisationUuid)
                    .Select(GetStsBrugerMunicipalityCvrFromUuid)
                    .Match
                    (
                        onSuccess: municipalityCvr =>
                            new StsBrugerInfo(
                                uuid,
                                emailsResult.Value,
                                Guid.Parse(organisationUuid),
                                municipalityCvr,
                                personData.Value.FirstName,
                                personData.Value.LastName),
                        onFailure: error =>
                        {
                            _logger.Error("Failed to resolve CVR '{error}'", error);
                            return Maybe<StsBrugerInfo>.None;
                        }
                    );
        }

        private Result<(string emailAdresseUuid, string organisationUuid, string personUuid), string> CollectStsBrugerInformationFromUuid(Guid uuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsBrugerHelpers.CreateBrugerPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformBrugerService, clientCertificate);
                var laesRequest = StsBrugerHelpers.CreateStsBrugerLaesRequest(_authorizedMunicipalityCvr, uuid);
                var brugerPortType = client.ChannelFactory.CreateChannel();
                var laesResponseResult = brugerPortType.laes(laesRequest).FromNullable();

                if (laesResponseResult.IsNone)
                    return $"Failed to fetch data from STS Bruger from uuid {uuid}";

                var registrations =
                    laesResponseResult
                        .Select(x=>x.LaesResponse1)
                        .Select(x => x.LaesOutput)
                        .Select(x => x.FiltreretOejebliksbillede)
                        .Select(x => x.Registrering);

                if (registrations.IsNone)
                {
                    var errorMessage = 
                        laesResponseResult
                            .Select(x => x.LaesResponse1)
                            .Select(x => x.LaesOutput)
                            .Select(x => x.StandardRetur)
                            .Select(x => $"{x.StatusKode ?? ""}:{x.FejlbeskedTekst ?? ""}")
                            .GetValueOrFallback("Unknown");

                    return $"Failed to parse STS Bruger registrations from UUID {uuid}. Error from STS Bruger: {errorMessage}";
                }

                foreach (var registreringType1 in registrations.Value)
                {
                    if (registreringType1.IsStsBrugerObsolete())
                    {
                        //User info obolete, go to next registration
                        continue;
                    }

                    var organizationUuid =
                        registreringType1
                            .RelationListe
                            .FromNullable()
                            .Select(x => x.Tilhoerer)
                            .Select(x => x.ReferenceID)
                            .Select(x => x.Item);

                    if (organizationUuid.IsNone)
                    {
                        //No organization uuid
                        continue;
                    }
                    string emailUuid = null;
                    var adresses =
                        registreringType1
                            .RelationListe
                            .FromNullable()
                            .Select(x => x.Adresser)
                            .GetValueOrFallback(new AdresseFlerRelationType[0]);

                    foreach (var adresse in adresses)
                    {
                        var emailField =
                            adresse
                                .Rolle
                                .FromNullable()
                                .Select(x => x.Item)
                                .GetValueOrFallback(string.Empty);

                        if (EmailTypeIdentifier.Equals(emailField))
                        {
                            emailUuid = adresse
                                .ReferenceID
                                .FromNullable()
                                .Select(x => x.Item)
                                .GetValueOrDefault();

                            if (emailUuid == null)
                            {
                                //No uuid provided
                                continue;
                            }
                            break;
                        }
                    }

                    if (emailUuid == null)
                    {
                        //Email UUID could not be found - go to the next
                        continue;
                    }

                    var lastKnownPerson = registreringType1
                        .RelationListe
                        .FromNullable()
                        .Select(x => x.TilknyttedePersoner)
                        .Select(x => x.OrderByDescending(p => p.Virkning.TilTidspunkt))
                        .Select(x => x.FirstOrDefault())
                        .GetValueOrDefault()
                        .FromNullable();

                    if (lastKnownPerson.IsNone)
                    {
                        //Unable to fetch data from last known person
                        continue;
                    }

                    var personUuid = lastKnownPerson.Select(x => x.ReferenceID).Select(x => x.Item);
                    if (personUuid.IsNone)
                    {
                        //Person UUID could not be found, bail out
                        continue;
                    }

                    return (emailUuid, organizationUuid.Value, personUuid.Value);
                }
                return $"Unable to resolve email and organization UUID from uuid {uuid}";
            }
        }

        private Result<IEnumerable<string>, string> GetStsAdresseEmailFromUuid(string emailAdresseUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsAdresseHelpers.CreateAdressePortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformAdresseService, clientCertificate);
                var laesRequest = StsAdresseHelpers.CreateStsAdresseLaesRequest(_authorizedMunicipalityCvr, emailAdresseUuid);
                var adressePortType = client.ChannelFactory.CreateChannel();
                var laesResponse = adressePortType.laes(laesRequest).FromNullable();

                if (laesResponse.IsNone)
                    return $"Failed to read STS adresse from emailAdresseUUID {emailAdresseUuid}";

                var registreringType1s =
                    laesResponse
                        .Select(x => x.LaesResponse1)
                        .Select(x => x.LaesOutput)
                        .Select(x => x.FiltreretOejebliksbillede)
                        .Select(x => x.Registrering);

                if (registreringType1s.IsNone)
                {
                    var errorMessage =
                        laesResponse
                            .Select(x => x.LaesResponse1)
                            .Select(x => x.LaesOutput)
                            .Select(x => x.StandardRetur)
                            .Select(x => $"{x.StatusKode ?? ""}:{x.FejlbeskedTekst ?? ""}")
                            .GetValueOrFallback("Unknown");

                    return $"Failed to parse registrations from laesResponse for emailAdresseUuid:{emailAdresseUuid}. STS Returned error: {errorMessage}";
                }

                var result = new List<string>();
                foreach (var registreringType1 in registreringType1s.Value)
                {
                    if (registreringType1.IsStsAdresseObsolete())
                        continue;

                    var latest = registreringType1
                        .AttributListe
                        .FromNullable()
                        .Select(x => x.OrderByDescending(y => y.Virkning.TilTidspunkt))
                        .GetValueOrDefault()
                        ?.FirstOrDefault();

                    if (latest == null)
                    {
                        //Failed to parse latest adresse
                        continue;
                    }
                    result.Add(latest.AdresseTekst);
                }

                if (result.Any())
                    return result;

                return $"No email addresses found from emailAdresseUuid {emailAdresseUuid}";
            }
        }

        private Result<string, string> GetStsVirksomhedFromUuid(string organisationUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsOrganisationHelpers.CreateOrganisationPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformOrganisationService, clientCertificate);
                var laesRequest = StsOrganisationHelpers.CreateStsOrganisationLaesRequest(_authorizedMunicipalityCvr, organisationUuid);
                var organisationPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = organisationPortType.laes(laesRequest).FromNullable();

                if (laesResponse.IsNone)
                    return Result<string, string>.Failure($"Failed to read virksomhed from org uuid {organisationUuid}");

                var registreringType1s =
                    laesResponse
                        .Select(x=>x.LaesResponse1)
                        .Select(x => x.LaesOutput)
                        .Select(x => x.FiltreretOejebliksbillede)
                        .Select(x => x.Registrering);

                if (registreringType1s.IsNone)
                {
                    var errorMessage =
                        laesResponse
                            .Select(x => x.LaesResponse1)
                            .Select(x => x.LaesOutput)
                            .Select(x => x.StandardRetur)
                            .Select(x => $"{x.StatusKode ?? ""}:{x.FejlbeskedTekst ?? ""}")
                            .GetValueOrFallback("Unknown");

                    return Result<string, string>.Failure($"Failed to read virksomhed registrations from org uuid {organisationUuid}. Error message:{errorMessage}");
                }

                foreach (var registreringType1 in registreringType1s.Value)
                {
                    if (registreringType1.IsStsOrganisationObsolete())
                        continue;

                    var referenceIdItem =
                        registreringType1
                            .RelationListe
                            .FromNullable()
                            .Select(x => x.Virksomhed)
                            .Select(x => x.ReferenceID)
                            .Select(x => x.Item);

                    if (referenceIdItem.IsNone)
                        continue;

                    return Result<string, string>.Success(referenceIdItem.Value);
                }

                return Result<string, string>.Failure($"Unable to find virksomhed uuid from org uuid {organisationUuid}");
            }
        }

        private Result<string, string> GetStsBrugerMunicipalityCvrFromUuid(string virksomhedUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsVirksomhedHelpers.CreateVirksomhedPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformVirksomhedService, clientCertificate);
                var laesRequest = StsVirksomhedHelpers.CreateStsVirksomhedLaesRequest(_authorizedMunicipalityCvr, virksomhedUuid);
                var virksomhedPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = virksomhedPortType.laes(laesRequest).FromNullable();

                if (laesResponse.IsNone)
                    return Result<string, string>.Failure($"Unable to read from STS Virksomhed with virksomhedUuid:{virksomhedUuid}");

                var registreringType1s =
                    laesResponse
                        .Select(x=>x.LaesResponse1)
                        .Select(x => x.LaesOutput)
                        .Select(x => x.FiltreretOejebliksbillede)
                        .Select(x => x.Registrering);

                if (registreringType1s.IsNone)
                {
                    var errorMessage =
                        laesResponse
                            .Select(x => x.LaesResponse1)
                            .Select(x => x.LaesOutput)
                            .Select(x => x.StandardRetur)
                            .Select(x => $"{x.StatusKode ?? ""}:{x.FejlbeskedTekst ?? ""}")
                            .GetValueOrFallback("Unknown");

                    return Result<string, string>.Failure($"Unable to read registrations from STS Virksomhed with virksomhedUuid:{virksomhedUuid}. Error message: {errorMessage}");
                }

                foreach (var registreringType1 in registreringType1s.Value)
                {
                    if (registreringType1.IsStsVirksomhedObsolete())
                        continue;

                    var latest =
                        registreringType1
                            .AttributListe
                            .FromNullable()
                            .Select(x => x.OrderByDescending(y => y.Virkning.TilTidspunkt))
                            .GetValueOrDefault()
                            ?.FirstOrDefault();

                    if (latest == null)
                        continue;

                    return Result<string, string>.Success(latest.CVRNummerTekst);
                }
                return Result<string, string>.Failure($"Unable to resolve cvr from virksomhedUuid:{virksomhedUuid}");
            }
        }

        private Result<StsPersonData, string> GetStsPersonFromUuid(string personUuid)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsPersonHelpers.CreatePersonPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformPersonService, clientCertificate);
                var laesRequest = StsPersonHelpers.CreateStsPersonLaesRequest(_authorizedMunicipalityCvr, personUuid);
                var virksomhedPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = virksomhedPortType.laes(laesRequest).FromNullable();

                if (laesResponse.IsNone)
                    return $"Failed to read from STS Person with personUUID:{personUuid}";

                var registreringType1s =
                    laesResponse
                        .Select(x => x.LaesResponse1)
                        .Select(x => x.LaesOutput)
                        .Select(x => x.FiltreretOejebliksbillede)
                        .Select(x => x.Registrering);

                if (registreringType1s.IsNone)
                {
                    var errorMessage =
                        laesResponse
                            .Select(x => x.LaesResponse1)
                            .Select(x => x.LaesOutput)
                            .Select(x => x.StandardRetur)
                            .Select(x => $"{x.StatusKode ?? ""}:{x.FejlbeskedTekst ?? ""}")
                            .GetValueOrFallback("Unknown");

                    return $"Failed to parse registrations from STS Person with personUUID:{personUuid}. Error message: {errorMessage}";
                }

                foreach (var registreringType1 in registreringType1s.Value)
                {
                    if (registreringType1.IsStsPersonObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1
                        .AttributListe
                        .FromNullable()
                        .Select(x => x.OrderByDescending(y => y.Virkning.TilTidspunkt))
                        .GetValueOrDefault()?
                        .FirstOrDefault();

                    if (latest == null)
                        continue;

                    return Result<StsPersonData, string>.Success(new StsPersonData(latest.NavnTekst));
                }
                return Result<StsPersonData, string>.Failure($"Unable to resolve person from personuuid:{personUuid}");
            }
        }

        private static BasicHttpBinding CreateHttpBinding()
        {
            return new BasicHttpBinding
            {
                Security =
                {
                    Mode = BasicHttpSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                },
                MaxReceivedMessageSize = int.MaxValue,
                OpenTimeout = new TimeSpan(0, 3, 0),
                CloseTimeout = new TimeSpan(0, 3, 0),
                ReceiveTimeout = new TimeSpan(0, 3, 0),
                SendTimeout = new TimeSpan(0, 3, 0)
            };
        }

        private static X509Certificate2 GetClientCertificate(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2 result;
                try
                {
                    var results = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    if (results.Count == 0)
                    {
                        throw new Exception("Unable to find certificate!");
                    }
                    result = results[0];
                }
                finally
                {
                    store.Close();
                }
                return result;
            }
        }
    }
}
