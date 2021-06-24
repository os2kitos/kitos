using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Infrastructure.Services.Types;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Denies access for users who act on behalf of rights holders.
    /// Reason is to ensure that rights holder access credentials are only user for stuff that's relevant to them
    /// </summary>
    public class DenyRightsHoldersAccessAttribute : ActionFilterAttribute
    {
        private readonly string _alternativeRoute;

        public DenyRightsHoldersAccessAttribute(string alternativeRoute = null)
        {
            _alternativeRoute = alternativeRoute;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IOrganizationalUserContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IOrganizationalUserContext));

            if (authContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
            {
                actionContext.Response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    Content = new StringContent(GetErrorMessage())
                };

            }
            base.OnActionExecuting(actionContext);
        }

        private string GetErrorMessage()
        {
            return _alternativeRoute?.Transform(route => $"Users with assigned rights holders access in one or more organizations must use '{_alternativeRoute}'") ??
                   "Users with assigned rights holders access in one or more organizations are not allowed to use this API. Please refer to the rights holders guide on KITOS Confluence or reach out to info@kitos.dk for assistance";
        }
    }
}