using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authentication;

namespace Presentation.Web.Infrastructure.Attributes
{
    public class PublicApiAttribute : ActionFilterAttribute
    {
        private readonly bool _onlyAccessWithKitosToken;

        public PublicApiAttribute(bool onlyAccessWithKitosToken = false)
        {
            _onlyAccessWithKitosToken = onlyAccessWithKitosToken;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (_onlyAccessWithKitosToken)
            {
                var authContext = (IAuthenticationContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IAuthenticationContext));

                if (authContext.Method != AuthenticationMethod.KitosToken)
                {
                    actionContext.Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Forbidden,
                        Content = new StringContent("Dette endpoint er kun for API brugere. Kontakt venligst KITOS Sekretariatet for oprettelse af API adgang.")
                    };

                }
            }
            base.OnActionExecuting(actionContext);
        }
    }
}