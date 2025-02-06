using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserReferenceWithOrganizationResponseDTO : UserReferenceResponseDTO
    {
        [Required]
        public IdentityNamePairResponseDTO Organization { get; set; }

        public UserReferenceWithOrganizationResponseDTO(IdentityNamePairResponseDTO organization, Guid uuid, string name, string email)
            : base(uuid, name, email)
        {
            Organization = organization;
        }
    }
}