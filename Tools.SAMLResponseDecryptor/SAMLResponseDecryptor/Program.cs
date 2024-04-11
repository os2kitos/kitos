using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

Console.WriteLine("Enter the SAML response symmetric key (<e:CipherValue>):");
var key = Console.ReadLine();
var decryptedSymmetricKey = SAMLDecryptor.DecryptSymmetricKey(key, @"C:\Users\MortenHoffmann\Strongminds Dropbox\Strongminds Team Folder\Projects\Ballerup Kommune\OS2Kitos Videreudvikling\03_Maintenance and support\03_IT Hosting\Certifikater\2023\OCES3\KITOS-PRODUCTION\kitos-production.pfx");

Console.WriteLine("Enter the SAML response assertion (<xenc:CipherValue>):");
var data = Console.ReadLine();
var decryptedAssertion = SAMLDecryptor.DecryptAssertion(data, decryptedSymmetricKey);
Console.WriteLine("---");
Console.WriteLine("Extracting privilege...");
var xml = new XmlDocument();
xml.LoadXml(decryptedAssertion);
var nsmgr = new XmlNamespaceManager(xml.NameTable);
nsmgr.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
nsmgr.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
nsmgr.AddNamespace("xenc", "http://www.w3.org/2001/04/xmlenc#");
nsmgr.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
var node = xml.SelectSingleNode("//saml:Attribute[@Name='dk:gov:saml:attribute:Privileges_intermediate']/saml:AttributeValue", nsmgr);
if (node == null)
{
    Console.WriteLine("No privileges found");
}
else
{
    var attributeValue = node.InnerText;
    var baseDecodedPrivilege = Convert.FromBase64String(attributeValue);
    string decodedPrivilege = Encoding.UTF8.GetString(baseDecodedPrivilege);
    Console.WriteLine($"Privilege={decodedPrivilege}"); 
}
Console.ReadLine();

public class SAMLDecryptor
{
    public static byte[] DecryptSymmetricKey(string encryptedKeyBase64, string privateKeyPath)
    {
        // Load your private key
        var certificate = new X509Certificate2(privateKeyPath, "-3,SfQYy7.aN", X509KeyStorageFlags.Exportable);
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

        using var aes = new AesCryptoServiceProvider();
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