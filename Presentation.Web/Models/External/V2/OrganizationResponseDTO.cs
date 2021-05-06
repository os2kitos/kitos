using System;

namespace Presentation.Web.Models.External.V2
{
    public class OrganizationResponseDTO: IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Organizational CVR identifier, if any
        /// </summary>
        public string Cvr { get; set; }

        public OrganizationResponseDTO(Guid uuid, string name) : base(uuid, name)
        {
        }
    }
}