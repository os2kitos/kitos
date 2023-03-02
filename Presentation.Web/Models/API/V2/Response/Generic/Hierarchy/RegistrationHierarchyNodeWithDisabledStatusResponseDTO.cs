using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.Generic.Hierarchy
{
    public class RegistrationHierarchyNodeWithDisabledStatusResponseDTO : RegistrationHierarchyNodeResponseDTO, IHasDeactivatedExternal
    {
        /// <summary>
        /// Active status of the node
        /// </summary>
        public bool Deactivated { get; set; }
    }
}