using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationUnitService
    {
        Result<UnitAccessRights, OperationError> GetAccessRights(Guid organizationUuid, Guid unitUuid);

        Result<IEnumerable<UnitAccessRightsWithUnitData>, OperationError> GetAccessRightsByOrganization(
            Guid organizationUuid);

        Result<OrganizationUnitRegistrationDetails, OperationError> GetRegistrations(Guid organizationUuid,
            Guid unitUuid);

        Maybe<OperationError> Delete(Guid organizationUuid, Guid unitUuid);

        Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid,
            OrganizationUnitRegistrationChangeParameters parameters);

        Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid);

        Maybe<OperationError> TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid,
            OrganizationUnitRegistrationChangeParameters parameters);

        Result<OrganizationUnit, OperationError> Create(Guid organizationUuid, Guid parentUuid,
            string name, OrganizationUnitOrigin origin);

        Result<Organization, OperationError> GetOrganizationAndAuthorizeModification(Guid uuid);

        Result<IEnumerable<OrganizationUnitRight>, OperationError> GetRightsOfUnitSubtree(Guid organizationUuid, Guid unitUuid);

        Result<OrganizationUnitRight, OperationError> CreateRoleAssignment(Guid organizationUnitUuid, Guid roleUuid,
            Guid userUuid);

        Result<OrganizationUnit, OperationError> CreateBulkRoleAssignment(Guid organizationUnitUuid,
            IEnumerable<UserRolePair> assignments);
        Result<OrganizationUnitRight, OperationError> DeleteRoleAssignment(Guid organizationUnitUuid, Guid roleUuid,
            Guid userUuid);

    }
}
