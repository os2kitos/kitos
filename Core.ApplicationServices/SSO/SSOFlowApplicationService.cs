using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using LivscyklusKodeType = Infrastructure.Soap.STSAdresse.LivscyklusKodeType;

namespace Core.ApplicationServices.SSO
{
    public class SSOFlowApplicationService
    {
        private const string UrlServicePlatformBrugerService = "https://exttest.serviceplatformen.dk/service/Organisation/Bruger/5";
        private const string UrlServicePlatformAdresseService = "https://exttest.serviceplatformen.dk/service/Organisation/Adresse/5";
        private const string CertificateThumbprint = "1793d097f45b0acea258f7fe18d5a4155799da26";
        private const string EmailTypeIdentifier = "5d13e891-162a-456b-abf2-fd9b864df96d";
        private const string MunicipalityCvr = "58271713"; // Ballerup CVR

        public IEnumerable<string> GetStsBrugerEmails(string uuid)
        {
            var emailAdresseUuid = GetStsBrugerEmailAdresseUuid(uuid);
            return GetStsAdresseEmailFromUuid(emailAdresseUuid);
        }

        private static string GetStsBrugerEmailAdresseUuid(string uuid)
        {
            var client = StsBrugerHelpers.CreateBrugerPortTypeClient(CreateHttpBinding(), UrlServicePlatformBrugerService, GetClientCertificate(CertificateThumbprint));
            var laesRequest = StsBrugerHelpers.CreateStsBrugerLaesRequest(MunicipalityCvr, uuid);
            var brugerPortType = client.ChannelFactory.CreateChannel();
            var laesResponse = brugerPortType.laes(laesRequest);
            var registreringType1 = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering[0];
            foreach (var adresse in registreringType1.RelationListe.Adresser)
            {
                if (EmailTypeIdentifier.Equals(adresse.Rolle.Item))
                {
                    return adresse.ReferenceID.Item;
                }
            }
            return string.Empty;
        }

        private static IEnumerable<string> GetStsAdresseEmailFromUuid(string emailAdresseUuid)
        {
            var client = StsAdresseHelpers.CreateAdressePortTypeClient(CreateHttpBinding(), UrlServicePlatformAdresseService, GetClientCertificate(CertificateThumbprint));
            var laesRequest = StsAdresseHelpers.CreateStsAdresseLaesRequest(MunicipalityCvr, emailAdresseUuid);
            var adressePortType = client.ChannelFactory.CreateChannel();
            var laesResponse = adressePortType.laes(laesRequest);
            var registreringType1s = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering;
            var result = new List<string>();
            foreach (var registreringType1 in registreringType1s)
            {
                if (registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Slettet) ||
                    registreringType1.LivscyklusKode.Equals(LivscyklusKodeType.Passiveret))
                {
                    continue;
                }
                var latest = registreringType1.AttributListe.OrderByDescending(a => a.Virkning.TilTidspunkt).First();
                result.Add(latest.AdresseTekst);
            }
            return result;
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
            X509Certificate2 result = null;
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
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
