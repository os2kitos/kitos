using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class SAMLDecryptor
{
    public static byte[] DecryptSymmetricKey(string encryptedKeyBase64, string privateKeyPath, string certificatePassword)
    {
        // Load your private key
        var certificate = new X509Certificate2(privateKeyPath, certificatePassword, X509KeyStorageFlags.Exportable);
        var rsa = certificate.GetRSAPrivateKey();

        // Convert the base64-encoded encrypted symmetric key to byte array
        byte[] encryptedKeyBytes = Convert.FromBase64String(encryptedKeyBase64);

        // Decrypt the symmetric key
        byte[] symmetricKey = rsa.Decrypt(encryptedKeyBytes, RSAEncryptionPadding.OaepSHA1);

        return symmetricKey;
    }

    public static string DecryptAssertion(string encryptedAssertionBase64, byte[] symmetricKey)
    {
        // Convert the base64-encoded encrypted assertion to byte array
        byte[] encryptedAssertionBytes = Convert.FromBase64String(encryptedAssertionBase64);

        using (var aes = new AesCryptoServiceProvider())
        {
            aes.Key = symmetricKey;
            aes.Mode = CipherMode.CBC; // Assuming the IV is prepended to the cipher text
            aes.Padding = PaddingMode.PKCS7;

            // Extract the IV from the beginning of the encrypted data
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(encryptedAssertionBytes, iv, iv.Length);
            byte[] cipherText = new byte[encryptedAssertionBytes.Length - iv.Length];
            Array.Copy(encryptedAssertionBytes, iv.Length, cipherText, 0, cipherText.Length);

            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            using (var ms = new MemoryStream(cipherText))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var reader = new StreamReader(cs))
            {
                return reader.ReadToEnd(); // This is your decrypted assertion
            }
        }
    }
}