using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public class UnauthenticatedAuthorizationContext : IAuthorizationContext
    {
        public bool AllowReadsWithinOrganization(int organizationId)
        {
            return false;
        }

        public bool AllowReads(IEntity entity)
        {
            return false;
        }

        public bool AllowUpdates(IEntity entity)
        {
            return false;
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return false;
        }
    }
}