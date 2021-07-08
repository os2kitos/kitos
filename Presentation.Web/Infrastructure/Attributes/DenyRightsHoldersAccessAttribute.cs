using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Denies access for users who act on behalf of rights holders.
    /// Reason is to ensure that rights holder access credentials are only user for stuff that's relevant to them
    /// </summary>
    public class DenyRightsHoldersAccessAttribute : ActionFilterAttribute
    {
        private readonly string _alternativeRoute;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var authContext = (IOrganizationalUserContext)actionContext.ControllerContext.Configuration.DependencyResolver.GetService(typeof(IOrganizationalUserContext));

            if (authContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
            {
                if (!Skip(actionContext))
                    actionContext.Response = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Forbidden,
                        Content = new StringContent("Users with assigned rights holders access in one or more organizations are not allowed to use this API. Please refer to the rights holders guide on KITOS Confluence or reach out to info@kitos.dk for assistance")
                    };

            }
            base.OnActionExecuting(actionContext);
        }

        private static bool Skip(HttpActionContext actionContext) =>
            actionContext.ActionDescriptor.GetCustomAttributes<AllowRightsHoldersAccessAttribute>().Any() ||
            actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowRightsHoldersAccessAttribute>().Any();
    }
}