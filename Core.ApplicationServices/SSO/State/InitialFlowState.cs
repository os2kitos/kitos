using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Core.DomainModel.Result;
using Core.DomainServices.SSO;
using dk.nita.saml20.identity;

namespace Core.ApplicationServices.SSO.State
{
    public class InitialFlowState : AbstractState
    {
        private readonly IStsBrugerInfoService _stsBrugerInfoService;
        private readonly string _samlKitosReadAccessRoleIdentifier;

        public InitialFlowState(IStsBrugerInfoService stsBrugerInfoService, SsoFlowConfiguration configuration)
        {
            _stsBrugerInfoService = stsBrugerInfoService;
            _samlKitosReadAccessRoleIdentifier = $"{configuration.SamlEntityId}/roles/usersystemrole/readaccess/1";
        }

        public override void Handle(FlowEvent @event, FlowContext context)
        {
            if (@event.Equals(FlowEvent.LoginCompleted))
            {
                if (CurrentUserHasKitosPrivilege())
                {
                    var userUuid = GetCurrentUserUuid();
                    var stsBrugerInfo = _stsBrugerInfoService.GetStsBrugerInfo(userUuid);
                    context.TransitionTo(new LookupStsUserEmailState(stsBrugerInfo.Emails));
                    context.HandleUserHasValidAccessRoleInSamlToken();
                }
            }
        }

        private static string GetCurrentUserUuid()
        {
            foreach (var claim in Saml20Identity.Current.Claims)
            {
                var serials = claim.Value.Split(new[] { "Serial=" }, StringSplitOptions.RemoveEmptyEntries);
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

        private bool CurrentUserHasKitosPrivilege()
        {
            return
                Saml20Identity
                    .Current[StsAdgangsStyringConstants.Attributes.PrivilegeKey]
                    .FirstOrDefault()
                    .FromNullable()
                    .Select(samlAttribute =>
                    {
                        var decodedSamlPrivilege = DecodeSamlRequestString(samlAttribute.AttributeValue.First());
                        var samlPrivilegeAsXml = new XmlDocument();
                        samlPrivilegeAsXml.LoadXml(decodedSamlPrivilege);
                        Maybe<XmlNode> privilegeNode = samlPrivilegeAsXml.SelectSingleNode("//Privilege");
                        return privilegeNode.Select(x => x.InnerText.Contains(_samlKitosReadAccessRoleIdentifier)).GetValueOrFallback(false);
                    })
                    .GetValueOrFallback(false);
        }

        private static string DecodeSamlRequestString(string compressedData)
        {
            var memStream = new MemoryStream(Convert.FromBase64String(compressedData));
            return new StreamReader(memStream, Encoding.UTF8).ReadToEnd();
        }

    }
}