using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Core.DomainServices.SSO;
using dk.nita.saml20.identity;

namespace Core.ApplicationServices.SSO.State
{
    public class InitialFlowState : AbstractState
    {
        private readonly IStsBrugerEmailService _stsBrugerEmailService;

        public InitialFlowState(IStsBrugerEmailService stsBrugerEmailService)
        {
            _stsBrugerEmailService = stsBrugerEmailService;
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.LoginCompleted))
            {
                if (CurrentUserHasKitosPrivilege())
                {
                    var userUuid = GetCurrentUserUuid();
                    var stsBrugerEmails = _stsBrugerEmailService.GetStsBrugerEmails(userUuid);
                    context.TransitionTo(new LookupStsUserEmailState(stsBrugerEmails));
                    context.HandleUserHasValidAccessRoleInSamlToken();
                }
            }
        }

        private static string GetCurrentUserUuid()
        {
            foreach (var claim in Saml20Identity.Current.Claims)
            {
                var serials = claim.Value.Split(new[]{ "Serial=" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var serial in serials)
                {
                    if (Guid.TryParse(serial, out var userUuid))
                    {
                        return userUuid.ToString();
                    }
                }
            }
            return string.Empty;
        }

        private static bool CurrentUserHasKitosPrivilege()
        {
            const string samlKitosPrivilegeKey = "dk:gov:saml:attribute:Privileges_intermediate"; //TODO: to const class
            const string samlKitosReadAccessRoleIdentifier = "http://kitos-local.strongminds.dk/roles/usersystemrole/readaccess/1"; //TODO: replace entity id in this string..
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