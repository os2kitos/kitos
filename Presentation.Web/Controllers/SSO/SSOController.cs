using System.Web.Http;
using Castle.Core.Internal;
using Core.ApplicationServices.SSO;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController: ApiController
    {
        private readonly ISSOFlowApplicationService _ssoFlowApplicationService;

        public SSOController(ISSOFlowApplicationService ssoFlowApplicationService)
        {
            _ssoFlowApplicationService = ssoFlowApplicationService;
        }

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
                    if (_ssoFlowApplicationService.HasCurrentUserKitosPrivilege())
                    {
                        result = $"User '{currentIdentityName}' has Kitos read access";
                    }
                }
            }
            return Ok(result);
        }
    }
}