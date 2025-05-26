using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Tools.SAMLResponseDecryptor
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine(@"Enter the path for the certificate file (eg C:\Temp\MyCertificate.pfx, __no quotes__):");
            var certificatePath = Console.ReadLine();
            if (!File.Exists(certificatePath))
            {
                Console.WriteLine("Certificate file not found.");
                return;
            }
            Console.WriteLine("Enter the password for the certificate (from 1pwd):");
            var certificatePassword = Console.ReadLine();
            Console.WriteLine("Enter the SAML response symmetric key (<e:CipherValue>):");
            var key = Console.ReadLine();
            var decryptedSymmetricKey = SAMLDecryptor.DecryptSymmetricKey(key, certificatePath, certificatePassword);

            Console.WriteLine("Enter the SAML response assertion (<xenc:CipherValue>):");
            var data = Console.ReadLine();
            var decryptedAssertion = SAMLDecryptor.DecryptAssertion(data, decryptedSymmetricKey);
            Console.WriteLine("---");
            Console.WriteLine($"Decrypted assertion: {decryptedAssertion}");
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
                var decodedPrivilege = Encoding.UTF8.GetString(baseDecodedPrivilege);
                Console.WriteLine($"Privilege={decodedPrivilege}");
            }
            Console.ReadLine();
        }
    }
}