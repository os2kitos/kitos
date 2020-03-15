using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Xml;
using Castle.Core.Internal;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController: ApiController
    {
        [InternalApi]
        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            var result = "User not authenticated";
            if (Saml20Identity.IsInitialized())
            {
                var currentIdentityName = Saml20Identity.Current.Name;
                result = $"No Kitos privilegees for user '{currentIdentityName}'";
                var samlAttributes = Saml20Identity.Current["dk:gov:saml:attribute:Privileges_intermediate"];
                if (!samlAttributes.IsNullOrEmpty()) 
                {
                    if (HasCurrentUserKitosPrivilege())
                    {
                        result = $"User '{currentIdentityName}' has Kitos read access";
                    }
                }
            }
            return Ok(result);
        }

        private static bool HasCurrentUserKitosPrivilege()
        {
            const string samlKitosPrivilegeKey = "dk:gov:saml:attribute:Privileges_intermediate";
            const string samlKitosReadAccessRoleIdentifier = "http://kitos-local.strongminds.dk/roles/usersystemrole/readaccess/1";
            var result = false;
            if (Saml20Identity.Current.HasAttribute(samlKitosPrivilegeKey))
            {
                var samlAttribute = Saml20Identity.Current[samlKitosPrivilegeKey].First();
                var decodedSamlPrivilege = DecodeSamlRequestString(samlAttribute.AttributeValue.First());
                var samlPrivilegeAsXml = new XmlDocument();
                samlPrivilegeAsXml.LoadXml(decodedSamlPrivilege);
                var privilegeNode = samlPrivilegeAsXml.SelectSingleNode("//Privilege");
                if (privilegeNode != null && privilegeNode.InnerText.Contains(samlKitosReadAccessRoleIdentifier))
                {
                    result = true;
                }
            }
            return result;
        }

        private static string DecodeSamlRequestString(string compressedData) 
        {
            var memStream = new MemoryStream(Convert.FromBase64String(compressedData));
            return new StreamReader(memStream, Encoding.UTF8).ReadToEnd();
        }
    }
}