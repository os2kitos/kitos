using System;

namespace Presentation.Web.Models.External.V2
{
    public class OrganizationResponseDTO: IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Organizational CVR identifier, if any
        /// </summary>
        public string Cvr { get; }

        public OrganizationResponseDTO(Guid uuid, string name, string cvr) : base(uuid, name)
        {
            Cvr = cvr;
        }
    }
}