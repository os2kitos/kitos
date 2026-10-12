using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.STS.Common.Model.Token
{
    public static class CertificateLoader
    {
        private static string NormalizeThumbprint(string thumbprint)
        {
            return (thumbprint ?? string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        }

        /// <summary>
        /// Loads a certificate from a PFX file. Use this overload when running in environments
        /// without access to the Windows certificate store (e.g., Linux containers).
        /// </summary>
        public static X509Certificate2 LoadCertificateFromFile(string filePath, string password)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Certificate file path must not be empty.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Certificate file not found: '{filePath}'", filePath);

            return X509CertificateLoader.LoadPkcs12FromFile(filePath, password, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);
        }

        /// <summary>
        /// Loads a certificate by thumbprint, trying LocalMachine first and falling back to CurrentUser.
        /// Throws a descriptive exception if the certificate is not found or its private key container is missing.
        /// </summary>
        public static X509Certificate2 LoadCertificate(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            var normalizedThumbprint = NormalizeThumbprint(thumbprint);
            if (string.IsNullOrWhiteSpace(normalizedThumbprint))
                throw new ArgumentException("Certificate thumbprint must not be empty.", nameof(thumbprint));

            // Try the requested store first, then fall back to the other location
            var locationsToTry = storeLocation == StoreLocation.LocalMachine
                ? new[] { StoreLocation.LocalMachine, StoreLocation.CurrentUser }
                : new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

            foreach (var location in locationsToTry)
            {
                using var store = new X509Store(storeName, location);
                store.Open(OpenFlags.ReadOnly);
                var result = store.Certificates.Find(X509FindType.FindByThumbprint, normalizedThumbprint, false);
                if (result.Count > 0)
                {
                    var cert = result[0];
                    if (cert.HasPrivateKey)
                    {
                        try
                        {
                            // Eagerly verify the private key is accessible to surface a clear error
                            _ = cert.GetRSAPrivateKey() ?? cert.GetECDsaPrivateKey() as object;
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(
                                $"Certificate with thumbprint '{normalizedThumbprint}' was found in {location}\\{storeName} " +
                                $"but its private key is not accessible. Re-import the PFX file with its private key. " +
                                $"Inner error: {ex.Message}", ex);
                        }
                    }
                    return cert;
                }
            }

            throw new ArgumentException(
                $"No certificate with thumbprint '{normalizedThumbprint}' was found in {storeName} " +
                $"(tried LocalMachine and CurrentUser). Install the certificate and retry.");
        }

        /// <summary>
        /// Loads a certificate using file-based config if available, otherwise falls back to the Windows certificate store.
        /// </summary>
        /// <param name="certFilePath">Path to the PFX file (null/empty to skip file-based loading).</param>
        /// <param name="certPassword">Password for the PFX file.</param>
        /// <param name="storeName">Certificate store name for Windows fallback.</param>
        /// <param name="storeLocation">Certificate store location for Windows fallback.</param>
        /// <param name="thumbprint">Thumbprint for Windows certificate store lookup.</param>
        public static X509Certificate2 LoadCertificateWithFallback(
            string? certFilePath,
            string? certPassword,
            StoreName storeName,
            StoreLocation storeLocation,
            string thumbprint)
        {
            if (!string.IsNullOrWhiteSpace(certFilePath))
            {
                var cert = LoadCertificateFromFile(certFilePath, certPassword ?? string.Empty);
                var normalizedThumbprint = NormalizeThumbprint(thumbprint);
                if (!string.IsNullOrWhiteSpace(normalizedThumbprint))
                {
                    var certThumbprint = NormalizeThumbprint(cert.Thumbprint);
                    if (!string.Equals(certThumbprint, normalizedThumbprint, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            $"Certificate loaded from '{certFilePath}' has thumbprint '{certThumbprint}', " +
                            $"but '{normalizedThumbprint}' was configured. Ensure Docker/Kubernetes mounts the intended certificate file.");
                    }
                }
                return cert;
            }

            return LoadCertificate(storeName, storeLocation, thumbprint);
        }
    }
}
