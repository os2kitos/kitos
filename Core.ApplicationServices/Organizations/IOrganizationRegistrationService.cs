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
        public Result<OrganizationRegistrationsRoot, OperationError> GetOrganizationRegistrations(int organizationId);
        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, OrganizationRegistrationsChangeParameters parameters);
        public Maybe<OperationError> TransferSelectedOrganizationRegistrations();
    }
}
