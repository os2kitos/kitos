using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Controllers.API.V2.External.Generic;

public interface IOrganizationMapper
{
    OrganizationResponseDTO ToDTO(Organization organization);
}