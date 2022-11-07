using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;
using System;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationUnitService
    {
        Result<OrganizationRegistrationDetails, OperationError> GetRegistrations(Guid organizationUuid, Guid unitUuid);
        Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid, OrganizationRegistrationChangeParameters parameters);
        Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid);
        Maybe<OperationError> TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, OrganizationRegistrationChangeParameters parameters);
    }
}
