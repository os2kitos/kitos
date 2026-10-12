using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{
    public class UserReferenceWithOrganizationResponseDTO : UserReferenceResponseDTO
    {
        [Required]
        public required IdentityNamePairResponseDTO Organization { get; set; }

        [SetsRequiredMembers]
        public UserReferenceWithOrganizationResponseDTO(IdentityNamePairResponseDTO organization, Guid uuid, string name, string email)
            : base(uuid, name, email)
        {
            Organization = organization;
        }
    }
}