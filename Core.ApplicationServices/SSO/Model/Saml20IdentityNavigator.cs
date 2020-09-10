using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using dk.nita.saml20.identity;
using Infrastructure.Services.Types;
using SamlAttribute = dk.nita.saml20.Schema.Core.SamlAttribute;

namespace Core.ApplicationServices.SSO.Model
{
    public class Saml20IdentityNavigator
    {
        private readonly ISaml20Identity _sourceIdentity;

        public Saml20IdentityNavigator(ISaml20Identity sourceIdentity)
        {
            _sourceIdentity = sourceIdentity;
        }

        public IEnumerable<XmlNode> GetPrivilegeNodes()
        {
            return
                GetAttribute(StsAdgangsStyringConstants.Attributes.PrivilegeKey)
                    .Select(ToXml)
                    .Select(xml => xml.SelectNodes("//Privilege")?.OfType<XmlNode>())
                    .GetValueOrFallback(Enumerable.Empty<XmlNode>());
        }

        public Maybe<SamlAttribute> GetAttribute(string attributeName)
        {
            if (!_sourceIdentity.HasAttribute(attributeName))
            {
                return Maybe<SamlAttribute>.None;
            }
            return _sourceIdentity[attributeName].FirstOrDefault();
        }

        public Maybe<IEnumerable<string>> GetSerials()
        {
            var results = new List<string>();
            foreach (var claim in Saml20Identity.Current.Claims)
            {
                var serials = claim.Value.Split(new[] { "Serial=" }, StringSplitOptions.RemoveEmptyEntries);
                results.AddRange(serials);
            }

            return results.Any() ? results : Maybe<IEnumerable<string>>.None;
        }

        private static XmlDocument ToXml(SamlAttribute attribute)
        {
            var attributeAsString = DecodeSAMLRequestString(attribute.AttributeValue.First());
            var attributeXmlDocument = new XmlDocument();
            attributeXmlDocument.LoadXml(attributeAsString);
            return attributeXmlDocument;
        }

        private static string DecodeSAMLRequestString(string compressedData)
        {
            using (var memStream = new MemoryStream(Convert.FromBase64String(compressedData)))
            using (var reader = new StreamReader(memStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
