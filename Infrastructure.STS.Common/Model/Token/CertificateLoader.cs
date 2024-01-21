using System;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.STS.Common.Model.Token
{
    public static class CertificateLoader
    {
        public static X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);
            var result = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (result.Count == 0)
            {
                throw new ArgumentException("No certificate with thumbprint " + thumbprint + " is found.");
            }

            return result[0];
        }
    }
}
