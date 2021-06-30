using System;

namespace Presentation.Web.Models.External.V2.Response
{
    public class ShallowOrganizationResponseDTO : IdentityNamePairResponseDTO
    {
        /// <summary>
        /// Organizational CVR identifier, if any
        /// </summary>
        public string Cvr { get; }

        public ShallowOrganizationResponseDTO(Guid uuid, string name, string cvr) : base(uuid, name)
        {
            Cvr = cvr;
        }
    }
}