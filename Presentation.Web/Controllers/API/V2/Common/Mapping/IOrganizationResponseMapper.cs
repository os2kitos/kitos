using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.Common.Mapping;

public interface IOrganizationResponseMapper
{
    OrganizationResponseDTO ToOrganizationDTO(Organization organization);
    OrganizationMasterDataRolesResponseDTO ToRolesDTO(OrganizationMasterDataRoles roles);

    OrganizationMasterDataResponseDTO ToMasterDataDTO(Organization organization);

}