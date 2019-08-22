using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Presentation.Web.Access
{
    public interface IOrganizationalUserContext
    {
        int ActiveOrganizationId { get; }
        int UserId { get; }
        bool IsActiveInOrganizationOfType(OrganizationCategory category);
        bool HasRole(OrganizationRole role);
        bool HasModuleLevelAccessTo(IEntity entity);
        bool IsActiveInOrganization(int organizationId);
        bool IsActiveInSameOrganizationAs(IContextAware contextAwareOrg);
        bool HasAssignedWriteAccess(IEntity entity);
        bool HasOwnership(IEntity entity);
        bool CanChangeVisibilityOf(IEntity entity);
    }
}
