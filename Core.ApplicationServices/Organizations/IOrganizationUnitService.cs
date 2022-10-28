using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationUnitService
    {
        Result<OrganizationRegistrationDetails, OperationError> GetOrganizationRegistrations(int organizationId, int unitId);
        Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int organizationId, int unitId, OrganizationRegistrationChangeParameters parameters);
        Maybe<OperationError> DeleteAllUnitOrganizationRegistrations(int organizationId, int unitId);
        Maybe<OperationError> TransferSelectedOrganizationRegistrations(int organizationId, int unitId, int targetUnitId, OrganizationRegistrationChangeParameters parameters);
    }
}
