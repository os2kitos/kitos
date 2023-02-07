using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Generic.Hierarchy
{
    public class RegistrationHierarchyNodeResponseDTO
    {
        /// <summary>
        /// Current node in the hierarchy
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO Node { get; set; }
        /// <summary>
        /// Parent of the current node
        /// </summary>
        public IdentityNamePairResponseDTO Parent { get; set; }
    }
}