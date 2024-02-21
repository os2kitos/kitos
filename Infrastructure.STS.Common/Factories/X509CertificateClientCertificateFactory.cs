using System;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.STS.Common.Factories
{
    public static class X509CertificateClientCertificateFactory
    {
        public static X509Certificate2 GetClientCertificate(string thumbprint, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.LocalMachine)
        {
            using var store = new X509Store(storeName, storeLocation);

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
