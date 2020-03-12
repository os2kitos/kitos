using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Infrastructure.Soap.STSBruger;

namespace Core.ApplicationServices.SSO
{
    public class SSOFlowApplicationService
    {
        private const string HttpsExttestServiceplatformenDkServiceOrganisationBruger = "https://exttest.serviceplatformen.dk/service/Organisation/Bruger/5";
        private const string CertificateThumbprint = "1793d097f45b0acea258f7fe18d5a4155799da26";
        private const string MunicipalityCvr = "58271713"; // Ballerup CVR

        public string GetStsBrugerEmail(string uuid)
        {
            var binding = CreateHttpBinding();
            var client = CreateBrugerPortTypeClient(binding);
            var laesRequest = CreateRequest(uuid);
            var brugerPortType = client.ChannelFactory.CreateChannel();
            var laesResponse = brugerPortType.laes(laesRequest);
            var registreringType1 = laesResponse.LaesResponse1.LaesOutput.FiltreretOejebliksbillede.Registrering[0];
            var adresseFlerRelationTypes = registreringType1.RelationListe.Adresser;
            var emailUnikIdType = new UnikIdType {Item = "5d13e891-162a-456b-abf2-fd9b864df96d"};
            foreach (var adresseFlerRelationType in adresseFlerRelationTypes)
            {
                if (adresseFlerRelationType.ReferenceID.Equals(emailUnikIdType))
                {
                    return adresseFlerRelationType.ToString();
                }
            }
            return string.Empty;
        }

        private static laesRequest CreateRequest(string uuid)
        {
            var laesInputType = new LaesInputType {UUIDIdentifikator = uuid};
            var laesRequest = new laesRequest()
            {
                LaesRequest1 = new LaesRequestType()
                {
                    LaesInput = laesInputType,
                    AuthorityContext = new AuthorityContextType()
                    {
                        MunicipalityCVR = MunicipalityCvr 
                    }
                }
            };
            return laesRequest;
        }

        private static BrugerPortTypeClient CreateBrugerPortTypeClient(BasicHttpBinding binding)
        {
            var client = new BrugerPortTypeClient(binding, new EndpointAddress(HttpsExttestServiceplatformenDkServiceOrganisationBruger))
            {
                ClientCredentials =
                {
                    ClientCertificate =
                    {
                        Certificate = GetClientCertificate(CertificateThumbprint)
                    }
                }
            };
            return client;
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
