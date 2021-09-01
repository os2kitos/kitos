using System;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Organization
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