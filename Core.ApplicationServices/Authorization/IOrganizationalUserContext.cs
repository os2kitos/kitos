using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization
{
    public interface IOrganizationalUserContext
    {
        int ActiveOrganizationId { get; }
        int UserId { get; }
        User UserEntity { get; }
        bool IsActiveInOrganizationOfType(OrganizationCategory category);
        bool HasRole(OrganizationRole role);
        bool HasModuleLevelAccessTo(IEntity entity);
        bool IsActiveInOrganization(int organizationId);
        bool IsActiveInSameOrganizationAs(IEntity entity);
        bool HasAssignedWriteAccess(IEntity entity);
        bool HasOwnership(IEntity entity);
        bool CanChangeVisibilityOf(IEntity entity);
    }
}
