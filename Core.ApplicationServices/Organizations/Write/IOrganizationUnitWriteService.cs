using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.DomainModel.Organization;
using System;

namespace Core.ApplicationServices.Organizations.Write
{
    public interface IOrganizationUnitWriteService
    {
        Result<OrganizationUnit, OperationError> Create(Guid organizationUuid,
            OrganizationUnitUpdateParameters parameters);
    }
}
