using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class SAMLDecryptor
{
    public static byte[] DecryptSymmetricKey(string encryptedKeyBase64, string privateKeyPath, string certificatePassword)
    {
        // X509CertificateLoader is the non-obsolete replacement for the X509Certificate2 constructor (SYSLIB0057)
        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            privateKeyPath,
            certificatePassword,
            X509KeyStorageFlags.Exportable);

        // GetRSAPrivateKey() returns null if the certificate has no RSA private key (CS8602)
        var rsa = certificate.GetRSAPrivateKey()
            ?? throw new CryptographicException("Certificate does not contain an RSA private key.");

        byte[] encryptedKeyBytes = Convert.FromBase64String(encryptedKeyBase64);

        var paddingModes = new[]
        {
            (RSAEncryptionPadding.OaepSHA256, "OAEP SHA-256"),
            (RSAEncryptionPadding.OaepSHA1,   "OAEP SHA-1"),
            (RSAEncryptionPadding.Pkcs1,       "PKCS#1 v1.5"),
        };

        foreach (var (padding, name) in paddingModes)
        {
            try
            {
                byte[] symmetricKey = rsa.Decrypt(encryptedKeyBytes, padding);
                Console.WriteLine($"[OK] Decrypted with padding: {name}");
                return symmetricKey;
            }
            catch (CryptographicException)
            {
                Console.WriteLine($"[FAIL] Padding {name} did not work.");
            }
        }

        throw new CryptographicException("Failed to decrypt the symmetric key with any known RSA padding mode.");
    }

    public static string DecryptAssertion(string encryptedAssertionBase64, byte[] symmetricKey)
    {
        byte[] encryptedAssertionBytes = Convert.FromBase64String(encryptedAssertionBase64);

        // AES-256-GCM (xmlenc11): layout is [nonce:12][ciphertext:n][tag:16]
        const int nonceSize = 12; // GCM nonce: 96 bits
        const int tagSize = 16; // GCM auth tag: 128 bits

        if (encryptedAssertionBytes.Length < nonceSize + tagSize)
            throw new CryptographicException(
                $"Assertion data too short ({encryptedAssertionBytes.Length} bytes) to contain a GCM nonce and tag.");

        byte[] nonce = encryptedAssertionBytes[..nonceSize];
        byte[] tag = encryptedAssertionBytes[^tagSize..];
        byte[] cipherText = encryptedAssertionBytes[nonceSize..^tagSize];
        byte[] plainText = new byte[cipherText.Length];

        Console.WriteLine($"[DEBUG] GCM nonce={nonceSize}B, ciphertext={cipherText.Length}B, tag={tagSize}B");

        using var aesGcm = new AesGcm(symmetricKey, tagSize);
        aesGcm.Decrypt(nonce, cipherText, tag, plainText);

        return Encoding.UTF8.GetString(plainText);
    }
}