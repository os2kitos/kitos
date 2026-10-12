using System;

namespace Presentation.Web.Models.API.V2.Response.Organization;

public class ExternalOrganizationUnitResponseDTO : OrganizationUnitResponseDTO
{
    public Guid? ExternalOriginUuid { get; set; }
}
