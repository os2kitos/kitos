using System.Security.Cryptography.X509Certificates;

namespace Kombit.InfrastructureSamples
{
    public static class ConfigVariables
    {
        // Your Client Certificate (funktionscertifikat) 
        public const string ClientCertificateThumbprint = "5a96f4868fb67ef4829a91588a0cf0de4d2290ea"; // Insert your Client Certificate Thumbprint here, e.g. 3d69ddd9ec9bc99046f5b3637a9d7213385d9fbf

        // Certificate variables 

        // The alias for the certificate to validate responses from Organisation. The thumbprint and location is set in app.config.
        public const string ServiceCertificateAlias_ORG = "ORG_EXTTEST_Organisation_1";

        // The alias and thumbprint for the certificate to trust the STS.
        public const string StsCertificateAlias = "ADG_EXTTEST_Adgangsstyring_1";
        public const string StsCertificateThumbprint = "0aa7a193f18d095f7e2ce09d892178c9682b7924";

        // The STS issuer for token requests.
        public const string StsIssuer = "https://adgangsstyring.eksterntest-stoettesystemerne.dk/";

        // The endpoint of the STS (Secure Token Service).
        public const string StsEndpoint = StsIssuer + "runtime/services/kombittrust/14/certificatemixed";

        // Entity IDs for the Serviceplatform service to fetch token for and call.

        // Entity ID for Organisation 6
        public const string OrgService6EntityId = "http://stoettesystemerne.dk/service/organisation/3";
    }
}
