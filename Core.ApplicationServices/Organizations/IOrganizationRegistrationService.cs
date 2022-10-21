using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRegistrationService
    {
        Result<IEnumerable<OrganizationRegistrationDetails>, OperationError> GetOrganizationRegistrations(int unitId);
        Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, IEnumerable<OrganizationRegistrationChangeParameters> parameters);
        Maybe<OperationError> DeleteSingleOrganizationRegistration(int unitId, OrganizationRegistrationChangeParameters parameters);
        Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId);
        Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, IEnumerable<OrganizationRegistrationChangeParameters> parameters);
    }
}
