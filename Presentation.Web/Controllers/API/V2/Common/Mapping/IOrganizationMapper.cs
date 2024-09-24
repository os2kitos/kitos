using System;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.Model.Organizations.Write;
using Core.ApplicationServices.Model.Organizations.Write.MasterDataRoles;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.External.Generic;

public interface IOrganizationMapper
{
    OrganizationResponseDTO ToOrganizationDTO(Organization organization);
    OrganizationMasterDataRolesResponseDTO ToRolesDTO(OrganizationMasterDataRoles roles);
    OrganizationMasterDataUpdateParameters ToMasterDataUpdateParameters(OrganizationMasterDataRequestDTO dto);

    OrganizationMasterDataRolesUpdateParameters ToMasterDataRolesUpdateParameters(Guid organizationUuid,
        OrganizationMasterDataRolesRequestDTO dto);

    OrganizationMasterDataResponseDTO ToMasterDataDTO(Organization organization);

}