using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.DomainModel.Organization;
using System;

namespace Core.ApplicationServices.Organizations.Write
{
    public interface IOrganizationWriteService
    {
        Result<Organization, OperationError> UpdateMasterData(Guid organizationUuid,
            OrganizationMasterDataUpdateParameters parameters);
    }
}
