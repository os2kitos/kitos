using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.ApplicationServices.Model.Organizations.Write;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using System;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping
{
    public interface IOrganizationWriteModelMapper
    {
        OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto);
        OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto);
    }
}
