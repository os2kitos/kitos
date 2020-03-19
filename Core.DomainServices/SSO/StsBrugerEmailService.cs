using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Core.DomainModel.Result;
using Infrastructure.Soap.STSAdresse;
using RegistreringType1 = Infrastructure.Soap.STSBruger.RegistreringType1;

namespace Core.DomainServices.SSO
{
    public class StsBrugerEmailService : IStsBrugerEmailService
    {
        private readonly string _urlServicePlatformBrugerService;
        private readonly string _urlServicePlatformAdresseService;
        private readonly string _certificateThumbprint;
        private const string EmailTypeIdentifier = StsOrganisationConstants.UserProperties.Email;
        private readonly string _authorizedMunicipalityCvr;

        public StsBrugerEmailService(StsOrganisationIntegrationConfiguration configuration)
        {
            _certificateThumbprint = configuration.CertificateThumbprint;
            _urlServicePlatformBrugerService = $"https://{configuration.EndpointHost}/service/Organisation/Bruger/5";
            _urlServicePlatformAdresseService = $"https://{configuration.EndpointHost}/service/Organisation/Adresse/5";
            _authorizedMunicipalityCvr = configuration.AuthorizedMunicipalityCvr;
        }

        public IEnumerable<string> GetStsBrugerEmails(string uuid)
        {
            return
                GetStsBrugerEmailAdresseUuid(uuid)
                    .Select(GetStsAdresseEmailFromUuid)
                    .GetValueOrFallback(Enumerable.Empty<string>());
        }

        private Maybe<string> GetStsBrugerEmailAdresseUuid(string uuid)
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
                    if (IsObsolete(registreringType1))
                    {
                        continue;
                    }

                    foreach (var adresse in registreringType1.RelationListe.Adresser)
                    {
                        if (EmailTypeIdentifier.Equals(adresse.Rolle.Item))
                        {
                            return adresse.ReferenceID.Item;
                        }
                    }
                }

                return Maybe<string>.None;
            }
        }

        private static bool IsObsolete(RegistreringType1 registreringType1)
        {
            return registreringType1.LivscyklusKode == Infrastructure.Soap.STSBruger.LivscyklusKodeType.Slettet ||
                   registreringType1.LivscyklusKode == Infrastructure.Soap.STSBruger.LivscyklusKodeType.Passiveret;
        }

        private IEnumerable<string> GetStsAdresseEmailFromUuid(string emailAdresseUuid)
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
                    if (IsObsolete(registreringType1))
                    {
                        continue;
                    }

                    var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                    result.Add(latest.AdresseTekst);
                }

                return result;
            }
        }

        private static bool IsObsolete(Infrastructure.Soap.STSAdresse.RegistreringType1 registreringType1)
        {
            return registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Slettet) ||
                   registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret);
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
