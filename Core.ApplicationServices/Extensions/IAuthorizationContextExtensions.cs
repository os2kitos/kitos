using Core.ApplicationServices.Authorization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Extensions
{
    public static class AuthorizationContextExtensions
    {
        public static DataAccessLevel GetDataAccessLevel(this IAuthorizationContext context, int organizationId)
        {
            return new DataAccessLevel(context.GetCrossOrganizationReadAccess(), context.GetOrganizationReadAccessLevel(organizationId));
        }
    }
}
