using Presentation.Web.Models.API.V2.Types.Shared;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasRegistrationScope
    {
        /// <summary>
        /// Scope of the registration
        /// - Local: The scope of the registration is local to the organization in which is was created
        /// - Global: The scope of the registration is global to KITOS and can be accessed and associated by authorized clients
        /// </summary>
        [Required]
        public RegistrationScopeChoice Scope { get; set; }
    }
}
