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
        private const string StsStandardNotFoundResultCode = "44";

        private readonly string _urlServicePlatformBrugerService;
        private readonly string _urlServicePlatformAdresseService;
        private readonly string _urlServicePlatformPersonService;
        private readonly string _certificateThumbprint;

        public StsBrugerInfoService(StsOrganisationIntegrationConfiguration configuration, ILogger logger)
        {
            _logger = logger;
            _certificateThumbprint = configuration.CertificateThumbprint;
            _urlServicePlatformBrugerService = $"https://{configuration.EndpointHost}/service/Organisation/Bruger/5";
            _urlServicePlatformAdresseService = $"https://{configuration.EndpointHost}/service/Organisation/Adresse/5";
            _urlServicePlatformPersonService = $"https://{configuration.EndpointHost}/service/Organisation/Person/5";
        }

        public Maybe<StsBrugerInfo> GetStsBrugerInfo(Guid uuid, string cvrNumber)
        {
            var brugerInfo = CollectStsBrugerInformationFromUuid(uuid, cvrNumber);
            if (brugerInfo.Failed)
            {
                _logger.Error("Failed to resolve UUIDS '{error}'", brugerInfo.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            var (emailAdresseUuid, organisationUuid, personUuid) = brugerInfo.Value;
            var emailsResult = GetStsAdresseEmailFromUuid(emailAdresseUuid, cvrNumber);
            if (emailsResult.Failed)
            {
                _logger.Error("Failed to resolve Emails '{error}'", emailsResult.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            var personData = GetStsPersonFromUuid(personUuid, cvrNumber);
            if (personData.Failed)
            {
                _logger.Error("Failed to resolve Person '{error}'", personData.Error);
                return Maybe<StsBrugerInfo>.None;
            }

            return new StsBrugerInfo(
                uuid,
                emailsResult.Value,
                Guid.Parse(organisationUuid),
                cvrNumber,
                personData.Value.FirstName,
                personData.Value.LastName);
        }

        private Result<(string emailAdresseUuid, string organisationUuid, string personUuid), string> CollectStsBrugerInformationFromUuid(Guid uuid, string cvrNumber)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsBrugerHelpers.CreateBrugerPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformBrugerService, clientCertificate);
                var laesRequest = StsBrugerHelpers.CreateStsBrugerLaesRequest(cvrNumber, uuid);
                var brugerPortType = client.ChannelFactory.CreateChannel();
                var laesResponseResult = brugerPortType.laes(laesRequest);

                if (laesResponseResult == null)
                    return $"Failed to fetch data from STS Bruger from uuid {uuid}";

                var stdOutput = laesResponseResult.LaesResponse1?.LaesOutput?.StandardRetur;
                var returnCode = stdOutput?.StatusKode ?? "unknown";
                var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;

                if (returnCode == StsStandardNotFoundResultCode)
                    return $"Requested user '{uuid}' from cvr '{cvrNumber}' was not found. STS Bruger endpoint returned '{returnCode}:{errorCode}'";

                var registrations =
                    laesResponseResult
                        .LaesResponse1
                        ?.LaesOutput
                        ?.FiltreretOejebliksbillede
                        ?.Registrering
                        ?.ToList() ?? new List<RegistreringType1>();

                if (registrations.Any() == false)
                    return $"No STS Bruger registrations from UUID {uuid}";

                foreach (var registreringType1 in registrations)
                {
                    if (registreringType1.IsStsBrugerObsolete())
                    {
                        //User info obsolete, go to next registration
                        continue;
                    }

                    var organizationUuid =
                        registreringType1
                            .RelationListe
                            ?.Tilhoerer
                            ?.ReferenceID
                            ?.Item;

                    if (organizationUuid == null)
                    {
                        //No organization uuid
                        continue;
                    }
                    string emailUuid = null;
                    var adresses = registreringType1
                        .RelationListe
                        ?.Adresser
                        ?.ToList() ?? new List<AdresseFlerRelationType>();

                    foreach (var adresse in adresses)
                    {
                        var emailField = adresse.Rolle?.Item ?? string.Empty;

                        if (EmailTypeIdentifier.Equals(emailField))
                        {
                            emailUuid = adresse.ReferenceID?.Item;

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

                    var personUuid = registreringType1
                        .RelationListe
                        ?.TilknyttedePersoner
                        ?.OrderByDescending(p => p.Virkning.TilTidspunkt)
                        ?.FirstOrDefault()
                        ?.ReferenceID
                        ?.Item;


                    if (personUuid == null)
                    {
                        //Person UUID could not be found, bail out
                        continue;
                    }

                    return (emailUuid, organizationUuid, personUuid);
                }
                return $"Unable to resolve email and organization UUID from uuid {uuid}";
            }
        }

        private Result<IEnumerable<string>, string> GetStsAdresseEmailFromUuid(string emailAdresseUuid, string cvrNumber)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsAdresseHelpers.CreateAdressePortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformAdresseService, clientCertificate);
                var laesRequest = StsAdresseHelpers.CreateStsAdresseLaesRequest(cvrNumber, emailAdresseUuid);
                var adressePortType = client.ChannelFactory.CreateChannel();
                var laesResponse = adressePortType.laes(laesRequest);

                if (laesResponse == null)
                    return $"Failed to read STS Adresse from emailAdresseUUID {emailAdresseUuid}";

                var stdOutput = laesResponse.LaesResponse1?.LaesOutput?.StandardRetur;
                var returnCode = stdOutput?.StatusKode ?? "unknown";
                var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;

                if (returnCode == StsStandardNotFoundResultCode)
                    return $"Requested email address '{emailAdresseUuid}' from cvr '{cvrNumber}' was not found. STS Adresse endpoint returned '{returnCode}:{errorCode}'";

                var registreringType1s =
                    laesResponse
                        .LaesResponse1
                        ?.LaesOutput
                        ?.FiltreretOejebliksbillede
                        ?.Registrering ?? new Infrastructure.Soap.STSAdresse.RegistreringType1[0];

                if (registreringType1s.Any() == false)
                    return $"No registrations from laesResponse for emailAdresseUuid:{emailAdresseUuid}";

                var result = new List<string>();
                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsAdresseObsolete())
                        continue;

                    var latest = registreringType1
                        .AttributListe
                        ?.OrderByDescending(y => y.Virkning.TilTidspunkt)
                        ?.FirstOrDefault()
                        ?.AdresseTekst;

                    if (latest == null)
                    {
                        //Failed to parse latest adresse
                        continue;
                    }
                    result.Add(latest);
                }

                if (result.Any())
                    return result;

                return $"No email addresses found from emailAdresseUuid {emailAdresseUuid}";
            }
        }

        private Result<StsPersonData, string> GetStsPersonFromUuid(string personUuid, string cvrNumber)
        {
            using (var clientCertificate = GetClientCertificate(_certificateThumbprint))
            {
                var client = StsPersonHelpers.CreatePersonPortTypeClient(CreateHttpBinding(),
                    _urlServicePlatformPersonService, clientCertificate);
                var laesRequest = StsPersonHelpers.CreateStsPersonLaesRequest(cvrNumber, personUuid);
                var virksomhedPortType = client.ChannelFactory.CreateChannel();
                var laesResponse = virksomhedPortType.laes(laesRequest);

                if (laesResponse == null)
                    return $"Failed to read from STS Person with personUUID:{personUuid}";

                var stdOutput = laesResponse.LaesResponse1?.LaesOutput?.StandardRetur;
                var returnCode = stdOutput?.StatusKode ?? "unknown";
                var errorCode = stdOutput?.FejlbeskedTekst ?? string.Empty;

                if (returnCode == StsStandardNotFoundResultCode)
                    return $"Requested person '{personUuid}' from cvr '{cvrNumber}' was not found. STS Person endpoint returned '{returnCode}:{errorCode}'";

                var registreringType1s =
                    laesResponse
                        .LaesResponse1
                        ?.LaesOutput
                        ?.FiltreretOejebliksbillede
                        ?.Registrering ?? new Infrastructure.Soap.STSPerson.RegistreringType1[0];

                if (registreringType1s.Any() == false)
                {
                    return $"Failed to parse registrations from STS Person with personUUID:{personUuid}";
                }

                foreach (var registreringType1 in registreringType1s)
                {
                    if (registreringType1.IsStsPersonObsolete())
                    {
                        continue;
                    }

                    var latest = registreringType1
                        .AttributListe
                        ?.OrderByDescending(y => y.Virkning.TilTidspunkt)
                        ?.FirstOrDefault()
                        ?.NavnTekst;

                    if (latest == null)
                        continue;

                    return Result<StsPersonData, string>.Success(new StsPersonData(latest));
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
