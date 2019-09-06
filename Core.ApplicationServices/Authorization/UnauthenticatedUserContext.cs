using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization
{
    public class UnauthenticatedUserContext : IOrganizationalUserContext

    {
        private const int INVALID_ID = -1;

        public int ActiveOrganizationId { get; } = INVALID_ID;
        public int UserId { get; } = INVALID_ID;

        public bool IsActiveInOrganizationOfType(OrganizationCategory category)
        {
            return false;
        }

        public bool HasRole(OrganizationRole role)
        {
            return false;
        }

        public bool HasModuleLevelAccessTo(IEntity entity)
        {
            return false;
        }

        public bool IsActiveInOrganization(int organizationId)
        {
            return false;
        }

        public bool IsActiveInSameOrganizationAs(IEntity entity)
        {
            return false;
        }

        public bool HasAssignedWriteAccess(IEntity entity)
        {
            return false;
        }

        public bool HasOwnership(IEntity entity)
        {
            return false;
        }

        public bool CanChangeVisibilityOf(IEntity entity)
        {
            return false;
        }
    }
}
