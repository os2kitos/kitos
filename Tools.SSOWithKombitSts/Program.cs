using System.IO;
using System.Text;
using System.Xml;

namespace Tools.SSOWithKombitSts
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var samlMetadataTemplate = new XmlDocument {PreserveWhitespace = true};
            samlMetadataTemplate.Load("SamlMetadataTemplate.xml");
            var patchedCertificateSamlXml = PatchCertificatesInSamlMetadataXml(samlMetadataTemplate);
            var meta = new dk.nita.saml20.Saml20MetadataDocument(patchedCertificateSamlXml);
            var metaAsXmlString = meta.ToXml();
            using (var fileStream = new System.IO.StreamWriter("kitosids-saml-metadata.xml"))
            {
                fileStream.WriteLine(metaAsXmlString);
            }
        }

        private static XmlDocument PatchCertificatesInSamlMetadataXml(XmlDocument samlMetadataTemplate)
        {
            var namespaceManager = new XmlNamespaceManager(samlMetadataTemplate.NameTable);
            namespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var certificateNodes = samlMetadataTemplate.SelectNodes("//ds:X509Certificate", namespaceManager);
            var extractCertificateFromFile = ExtractCertificateFromFile("strongminds-kitos.cer");
            for (var i = 0; i < certificateNodes.Count; i++)
            {
                certificateNodes.Item(i).InnerText = extractCertificateFromFile;
            }
            return samlMetadataTemplate;
        }

        private static string ExtractCertificateFromFile(string filename)
        {
            var result = new StringBuilder();
            var lines = File.ReadAllLines("strongminds-kitos.cer");
            foreach (var line in lines)
            {
                if (line.StartsWith("---")) continue;
                result.Append(line);
            }
            return result.ToString();
        }
    }
}
