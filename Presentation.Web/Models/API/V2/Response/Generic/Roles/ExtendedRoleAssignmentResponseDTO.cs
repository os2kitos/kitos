using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.User;

namespace Presentation.Web.Models.API.V2.Response.Generic.Roles
{
    /// <summary>
    /// Internal response DTO used to extend the regular role assignment dto with extended user details (email)
    /// This information is only added to simplify UI cases, whereas the public interface is in line with the V2
    /// principles of not repeating data representation.
    /// </summary>
    public class ExtendedRoleAssignmentResponseDTO
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
        public UserReferenceResponseDTO Role { get; set; }
    }
}