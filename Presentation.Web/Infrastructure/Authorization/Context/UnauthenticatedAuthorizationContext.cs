using Core.DomainModel;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public class UnauthenticatedAuthorizationContext : IAuthorizationContext
    {
        public bool AllowGlobalReadAccess()
        {
            return false;
        }

        public bool AllowReadsWithinOrganization(int organizationId)
        {
            return false;
        }

        public bool AllowReads(IEntity entity)
        {
            return false;
        }

        public bool AllowCreate<T>()
        {
            return false;
        }

        public bool AllowCreate<T>(IEntity entity)
        {
            return false;
        }

        public bool AllowModify(IEntity entity)
        {
            return false;
        }

        public bool AllowDelete(IEntity entity)
        {
            return false;
        }

        public bool AllowEntityVisibilityControl(IEntity entity)
        {
            return false;
        }
    }
}