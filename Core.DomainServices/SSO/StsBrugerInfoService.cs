using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.DomainModel.Result;
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
                var laesResponse = brugerPortType.laes(laesRequest);
                foreach (var registreringType1 in laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering)
                {
                    if (registreringType1.IsStsBrugerObsolete())
                    {
                        continue;
                    }

                    var organizationUuid = registreringType1.RelationListe.Tilhoerer.ReferenceID.Item;
                    var emailUuid = string.Empty;
                    foreach (var adresse in registreringType1.RelationListe.Adresser)
                    {
                        if (EmailTypeIdentifier.Equals(adresse.Rolle.Item))
                        {
                            emailUuid = adresse.ReferenceID.Item;
                            break;
                        }
                    }

                    var lastKnownPerson = registreringType1.RelationListe.TilknyttedePersoner.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    var personUuid = lastKnownPerson.ReferenceID.Item;

                    return (emailUuid, organizationUuid, personUuid);
                }
                return "Unable to resolve email and organization UUID";
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
                var laesResponse = adressePortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                var result = new List<string>();
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsAdresseObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    result.Add(latest.AdresseTekst);
                }

                if (result.Any())
                    return result;

                return "No email addresses found";
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
                var laesResponse = organisationPortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsOrganisationObsolete())
                    {
                        continue;
                    }

                    return Result<string, string>.Success(registreringType1.RelationListe.Virksomhed.ReferenceID.Item);
                }
                return Result<string, string>.Failure("UUID not found");
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
                var laesResponse = virksomhedPortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsVirksomhedObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    return Result<string, string>.Success(latest.CVRNummerTekst);
                }
                return Result<string, string>.Failure("Unable to resolve cvr");
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
                var laesResponse = virksomhedPortType.laes(laesRequest);
                var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsPersonObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    return Result<StsPersonData, string>.Success(new StsPersonData(latest.NavnTekst));
                }
                return Result<StsPersonData, string>.Failure("Unable to resolve cvr");
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
