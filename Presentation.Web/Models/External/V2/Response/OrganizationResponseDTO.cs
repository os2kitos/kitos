using System;

namespace Presentation.Web.Models.External.V2.Response
{
    public class OrganizationResponseDTO: ShallowOrganizationResponseDTO
    {
        public OrganizationResponseDTO(Guid uuid, string name, string cvr) : base(uuid, name, cvr)
        {
        }
    }
}