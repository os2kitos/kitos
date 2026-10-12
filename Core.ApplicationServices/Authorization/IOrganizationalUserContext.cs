using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Authorization
{
    public interface IOrganizationalUserContext
    {
        int UserId { get; }
        IEnumerable<int> OrganizationIds { get; }
        bool HasRoleInOrganizationOfType(OrganizationCategory category);
        bool IsGlobalAdmin();
        bool HasStakeHolderAccess();
        IEnumerable<int> GetOrganizationIdsWhereHasRole(OrganizationRole role);
        bool HasRole(int organizationId, OrganizationRole role);
        bool HasRoleInAnyOrganization(OrganizationRole role);
        bool HasRoleIn(int organizationId);
        bool HasRoleInSameOrganizationAs(IEntity entity);
        bool IsSystemIntegrator();
        bool IsSupplierApiUserForOrganizationWithSuppliers(IEnumerable<int> supplierIds);
    }
}
