using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Generic.Hierarchy
{
    public class RegistrationHierarchyNodeResponseDTO
    {
        [Required]
        public IdentityNamePairResponseDTO Current { get; set; }
        public IdentityNamePairResponseDTO Parent { get; set; }
    }
}