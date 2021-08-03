using System;
using Presentation.Web.Models.API.V2.Types.Organization;

namespace Presentation.Web.Models.API.V2.Response.Organization
{
    public class OrganizationResponseDTO: ShallowOrganizationResponseDTO
    {
        public OrganizationType OrganizationType { get; }
        public OrganizationResponseDTO(Guid uuid, string name, string cvr, OrganizationType organizationType) : base(uuid, name, cvr)
        {
            OrganizationType = organizationType;
        }
    }
}