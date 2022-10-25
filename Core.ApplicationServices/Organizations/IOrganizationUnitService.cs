using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationUnitService
    {
        Result<OrganizationRegistrationDetails, OperationError> GetOrganizationRegistrations(int unitId);
        Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, OrganizationRegistrationChangeParameters parameters);
        Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId);
        Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, OrganizationRegistrationChangeParameters parameters);
    }
}
