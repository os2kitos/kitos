using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Presentation.Web.Infrastructure.Authorization.Context
{
    public class UnauthenticatedAuthorizationContext : IAuthorizationContext
    {
        public CrossOrganizationReadAccess GetCrossOrganizationReadAccess()
        {
            return CrossOrganizationReadAccess.None;
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