using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.DomainModel.Organization;
using System;

namespace Core.ApplicationServices.Organizations.Write
{
    public interface IOrganizationWriteService
    {
        Result<Organization, OperationError> UpdateMasterData(Guid organizationUuid,
            OrganizationMasterDataUpdateParameters parameters);

        Result<OrganizationMasterDataRoles, OperationError> UpsertOrganizationMasterDataRoles(Guid organizationUuid, OrganizationMasterDataRolesUpdateParameters updateParameters);
        
        public Result<OrganizationMasterDataRoles, OperationError>
            GetOrCreateOrganizationMasterDataRoles(Guid organizationUuid);
    }
}
