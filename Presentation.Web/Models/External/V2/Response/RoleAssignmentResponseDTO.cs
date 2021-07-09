using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.External.V2.Response
{
    /// <summary>
    /// Defines a generic assignment DTO representing the assignment of a role option to a KITOS User
    /// </summary>
    public class RoleAssignmentResponseDTO
    {
        /// <summary>
        /// KITOS user which the role has been assigned to
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO User { get; set; }
        /// <summary>
        /// Specific role option assigned to the user.
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO Role { get; set; }
    }
}