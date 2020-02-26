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
            if (args.Length != 1)
            {
                WriteUsageExplanation();
                Environment.Exit(-1);
            }

            var certificateFilename = args[0];
            var samlMetadataTemplate = new XmlDocument {PreserveWhitespace = true};
            samlMetadataTemplate.Load("SamlMetadataTemplate.xml");
            try
            {
                var patchedCertificateSamlXml = PatchCertificatesInSamlMetadataXml(samlMetadataTemplate, certificateFilename);
                var meta = new dk.nita.saml20.Saml20MetadataDocument(patchedCertificateSamlXml);
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
            Console.WriteLine("SSOWithKombitSts [file.cer]");
            Console.WriteLine();
            Console.WriteLine("file.cer\tFilename for public part of exported FOCES certificate (DER-binary X509 .cer format)");
        }

        private static XmlDocument PatchCertificatesInSamlMetadataXml(XmlDocument samlMetadataTemplate, string certificateFilename)
        {
            var namespaceManager = new XmlNamespaceManager(samlMetadataTemplate.NameTable);
            namespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var certificateNodes = samlMetadataTemplate.SelectNodes("//ds:X509Certificate", namespaceManager);
            var extractCertificateFromFile = ExtractCertificateFromFile(certificateFilename);
            for (var i = 0; i < certificateNodes.Count; i++)
            {
                certificateNodes.Item(i).InnerText = extractCertificateFromFile;
            }
            return samlMetadataTemplate;
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
