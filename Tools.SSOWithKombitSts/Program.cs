using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Tools.SSOWithKombitSts
{
    public class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                WriteUsageExplanation();
                return 1;
            }

            var certificateFilename = args[0];
            var entityId = args[1];
            var callbackUrl = args[2] ?? entityId;
            var samlMetadataTemplate = new XmlDocument {PreserveWhitespace = true};
            samlMetadataTemplate.Load("SamlMetadataTemplate.xml");
            try
            {
                var patchedCertificateSamlXml = PatchCertificatesInSamlMetadataXml(samlMetadataTemplate, certificateFilename);
                var patchedEntityIdSamlXml = PatchEntityIdInSamlMetadataXml(patchedCertificateSamlXml, entityId, callbackUrl);
                var meta = new dk.nita.saml20.Saml20MetadataDocument(patchedEntityIdSamlXml);
                var metaAsXmlString = meta.ToXml();
                const string samlMetadataXmlFilename = "kitosids-saml-metadata.xml";
                using (var fileStream = new StreamWriter(samlMetadataXmlFilename))
                {
                    fileStream.WriteLine(metaAsXmlString);
                    Console.WriteLine($"Output file '{samlMetadataXmlFilename}' written");
                    Console.WriteLine("Done.");
                }
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }

        private static void WriteUsageExplanation()
        {
            Console.WriteLine("SSOWithKombitSts outputs a KOMBIT compliant SAML configuration file");
            Console.WriteLine();
            Console.WriteLine("SSOWithKombitSts [file.cer] [entityID] {callbackUrl}");
            Console.WriteLine();
            Console.WriteLine("file.cer\tFilename for public part of exported FOCES certificate (Base64 X509 .cer format)");
            Console.WriteLine("entityID\tEntityID identifying system in KOMBIT STS Administration module (eg https://kitos-internal.strongminds.dk)");
            Console.WriteLine("callbackUrl\tServer for STS Organisation to callback, if different from entityID (eg https://kitostest.miracle.dk)");
            Console.WriteLine();
        }

        private static XmlDocument PatchCertificatesInSamlMetadataXml(XmlDocument samlMetadataTemplate, string certificateFilename)
        {
            var namespaceManager = GetXmlNamespaceManager(samlMetadataTemplate);
            var certificateNodes = samlMetadataTemplate.SelectNodes("//ds:X509Certificate", namespaceManager);
            var extractCertificateFromFile = ExtractCertificateFromFile(certificateFilename);
            for (var i = 0; i < certificateNodes.Count; i++)
            {
                certificateNodes.Item(i).InnerText = extractCertificateFromFile;
            }
            return samlMetadataTemplate;
        }

        private static XmlNamespaceManager GetXmlNamespaceManager(XmlDocument samlMetadataTemplate)
        {
          var namespaceManager = new XmlNamespaceManager(samlMetadataTemplate.NameTable);
          namespaceManager.AddNamespace("md", "urn:oasis:names:tc:SAML:2.0:metadata");
          namespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
          return namespaceManager;
        }

        private static XmlDocument PatchEntityIdInSamlMetadataXml(XmlDocument patchedCertificateSamlXml,
            string entityId, string callbackUrl)
        {
          var xmlNamespaceManager = GetXmlNamespaceManager(patchedCertificateSamlXml);
          var entityIdAttr = patchedCertificateSamlXml.SelectSingleNode("//md:EntityDescriptor/@entityID", xmlNamespaceManager);
          entityIdAttr.Value = entityId;
          var acsLocation = patchedCertificateSamlXml.SelectSingleNode("//md:AssertionConsumerService/@Location", xmlNamespaceManager);
          acsLocation.Value = callbackUrl + "/Login.ashx";
          return patchedCertificateSamlXml;
        }

        private static string ExtractCertificateFromFile(string certificateFilename)
        {
            try
            {
                var result = new StringBuilder();
                var lines = File.ReadAllLines(certificateFilename);
                foreach (var line in lines)
                {
                    if (line.StartsWith("---")) continue;
                    result.Append(line);
                }
                return result.ToString();
            }
            catch (FileNotFoundException)
            {
                throw new ApplicationException($"Error: Certificate file '{certificateFilename}' not found");
            }
        }
    }
}
