using System;
using System.Web.Http;
using System.Web.Http.Results;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Model;
using Core.ApplicationServices.SSO.State;
using Core.DomainModel.Result;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.SSO
{
    [RoutePrefix("SSO")]
    public class SSOController : ApiController
    {
        private readonly ISsoFlowApplicationService _ssoFlowApplicationService;
        private readonly Maybe<ISaml20Identity> _ssoSamlState;

        public SSOController(ISsoFlowApplicationService ssoFlowApplicationService, Maybe<ISaml20Identity> ssoSamlState)
        {
            _ssoFlowApplicationService = ssoFlowApplicationService;
            _ssoSamlState = ssoSamlState;
        }

        [InternalApi]
        [HttpGet]
        [Route("")]
        public IHttpActionResult SSO()
        {
            var result = "User not authenticated";
            if (_ssoSamlState.HasValue)
            {
                var currentIdentityName = _ssoSamlState.Value.Name;
                var finalState = _ssoFlowApplicationService.StartSsoLoginFlow();

                switch (finalState)
                {
                    case ErrorState errorState when errorState.ErrorCode.HasValue:
                        return SsoError(errorState.ErrorCode.Value);
                    case ErrorState errorState when errorState.ErrorCode.IsNone:
                        return SsoError(SsoErrorCode.Unknown);
                    case UserLoggedInState _:
                        //TODO: Redirect to front page.. user is logged in now
                        result = $"User '{currentIdentityName}' has Kitos read access";
                        break;
                }
            }
            return Ok(result);
        }

        private RedirectResult SsoError(SsoErrorCode error)
        {
            var uriBuilder = new UriBuilder(Request.RequestUri)
            {
                Path = "Home/SsoError",
                Query = $"ssoErrorCode={error:G}"
            };
            return Redirect(uriBuilder.Uri);
        }
    }
}