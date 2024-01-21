using System;
using System.Security.Cryptography.X509Certificates;

namespace Kombit.InfrastructureSamples
{
    public static class ConfigVariables
    {

        #region Variables which MUST BE MODIFIED before running the code examples

        // Your Client Certificate (funktionscertifikat) 
        public const string ClientCertificateThumbprint = "5a96f4868fb67ef4829a91588a0cf0de4d2290ea"; // Insert your Client Certificate Thumbprint here, e.g. 3d69ddd9ec9bc99046f5b3637a9d7213385d9fbf
        public const StoreLocation ClientCertificateStoreLocation = StoreLocation.LocalMachine; // Change if the certificate is stored in another location
        public const StoreName ClientCertificateStoreName = StoreName.My; // Change if the certificate is stored in another location

        // UUID and name of your it-system in Fælleskommunalt Administrationssystem
        public const string ANVENDER_SYSTEM_UUID = "ba537e12-8b0c-44b1-9de7-f75803a4e091"; // Change to the UUID of your system
        public const string ANVENDER_SYSTEM_NAVN = "STS testklient 29"; // Change to the name of your system

        // CVR and name of the municipality (myndighed) that will be used to test 
        public const string MYNDIGHEDS_CVR = "58271713"; // Change to your authority CVR
        public const string MYNDIGHEDS_NAVN = "Ballerup Kommune"; // Change to your authority name        

        // UUID used for the test case
        public const string UUID = "11111111-2222-3333-4444-555555555555";  // Generate your own UUID and insert it here 

        #endregion

        #region Variables for certificates and endpoints - CAN be modified 

        // Certificate variables 
        // The alias for the certificate to validate SP (Serviceplatformen) responses. The thumbprint and location is set in app.config.
        public const string ServiceCertificateAlias = "SDI_EXTTEST_Sags-og-Dokindeks_1";

        // The alias for the certificate to validate responses from Organisation. The thumbprint and location is set in app.config.
        public const string ServiceCertificateAlias_ORG = "ORG_EXTTEST_Organisation_1";

        // The alias for the certificate to validate responses from Klassifikation. The thumbprint and location is set in app.config.
        public const string ServiceCertificateAlias_KLA = "KLA_EXTTEST_Klassifikation_1";

        // The alias and thumbprint for the certificate to trust the STS.
        // Change only needed if you don't use the external test server for tokens.
        public const string StsCertificateAlias = "ADG_EXTTEST_Adgangsstyring_1";
        public const string StsCertificateThumbprint = "0aa7a193f18d095f7e2ce09d892178c9682b7924";

        public const StoreLocation StsCertificateStoreLocation = StoreLocation.CurrentUser;
        public const StoreName StsCertificateStoreName = StoreName.My;

        // The STS issuer for token requests.
        // Change only needed if you don't use the external test server for tokens.
        public const string StsIssuer = "https://adgangsstyring.eksterntest-stoettesystemerne.dk/";

        // The endpoint of the STS (Secure Token Service).
        public const string StsEndpoint = StsIssuer + "runtime/services/kombittrust/14/certificatemixed";

        // Entity IDs for the Serviceplatform service to fetch token for and call.
        // This ID can be found in the service contract package from the Serviceplatform as 'service.entityID' inside /sp/service.properties.
        public const string SagDokServiceEntityId = "http://entityid.kombit.dk/service/sdi/sagdokumentindeks/6";

        // Entity ID for Klassifikation 6
        public const string KlaServiceEntityId = "http://entityid.kombit.dk/service/klassifikation/7";

        // Entity ID for Organisation 6
        public const string OrgService6EntityId = "http://stoettesystemerne.dk/service/organisation/3";

        #endregion
    }
}
