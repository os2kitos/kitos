using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Presentation.Web.Access
{
    public interface IOrganizationalUserContext
    {
        User User { get; }
        bool IsActiveInOrganizationOfType(OrganizationCategory category);
        bool HasRole(OrganizationRole role);
        bool HasModuleLevelAccessTo(IEntity entity);
        bool IsActiveInOrganization(int organizationId);
        bool IsActiveInSameOrganizationAs(IContextAware contextAwareOrg);
    }
}
