using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Presentation.Web.Access
{
    public class AnonymouslUserContext : IOrganizationalUserContext
    {
        public User User { get; } = null;

        public int ActiveOrganizationId { get; } = -1;

        public int UserId { get; } = -1;

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

        public bool IsActiveInSameOrganizationAs(IContextAware contextAwareOrg)
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