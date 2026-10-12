using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.User
{

    /// <summary>
    /// Extended user reference response including the email property.
    /// This DTO should only be used on responses of internal convenience endpoints.
    /// </summary>
    public class UserReferenceResponseDTO : IdentityNamePairResponseDTO
    {
        protected UserReferenceResponseDTO()
        {
        }

        [SetsRequiredMembers]
        public UserReferenceResponseDTO(Guid uuid, string name, string email)
            : base(uuid, name)
        {
            Email = email;
        }

        [Required]
        public required string Email { get; set; }
    }
}