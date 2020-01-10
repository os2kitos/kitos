using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Authorization;

namespace Core.ApplicationServices.Authorization
{
    public class UnauthenticatedAuthorizationContext : IAuthorizationContext
    {
        public CrossOrganizationDataReadAccessLevel GetCrossOrganizationReadAccess()
        {
            return CrossOrganizationDataReadAccessLevel.None;
        }

        public OrganizationDataReadAccessLevel GetOrganizationReadAccessLevel(int organizationId)
        {
            return OrganizationDataReadAccessLevel.None;
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

        public bool AllowSystemUsageMigration()
        {
            return false;
        }

        public bool AllowBatchLocalImport()
        {
            return false;
        }

        public bool AllowChangeOrganizationType(OrganizationTypeKeys organizationType)
        {
            return false;
        }
    }
}