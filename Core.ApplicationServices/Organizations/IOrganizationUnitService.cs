using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationUnitService
    {
        Result<OrganizationRegistrationDetails, OperationError> GetOrganizationRegistrations(int unitId, int organizationId);
        Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, int organizationId, OrganizationRegistrationChangeParameters parameters);
        Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId, int organizationId);
        Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, int organizationId, OrganizationRegistrationChangeParameters parameters);
    }
}
