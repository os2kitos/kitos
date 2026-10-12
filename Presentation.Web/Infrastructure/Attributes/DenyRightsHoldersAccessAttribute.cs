using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.DomainModel.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Web.Infrastructure.Attributes
{
    /// <summary>
    /// Denies access for users who act on behalf of rights holders.
    /// Reason is to ensure that rights holder access credentials are only user for stuff that is relevant to them
    /// </summary>
    public class DenyRightsHoldersAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var authContext = actionContext.HttpContext.RequestServices
                .GetRequiredService<IOrganizationalUserContext>();

            if (authContext.HasRoleInAnyOrganization(OrganizationRole.RightsHolderAccess))
            {
                if (!Skip(actionContext))
                {
                    actionContext.Result = new ObjectResult("Users with assigned rights holders access in one or more organizations are not allowed to use this API. Please refer to the rights holders guide on KITOS Confluence or reach out to info@kitos.dk for assistance")
                    {
                        StatusCode = 403
                    };
                    return;
                }
            }

            base.OnActionExecuting(actionContext);
        }

        private static bool Skip(ActionExecutingContext actionContext) =>
            actionContext.ActionDescriptor.EndpointMetadata.OfType<AllowRightsHoldersAccessAttribute>().Any();
    }
}
