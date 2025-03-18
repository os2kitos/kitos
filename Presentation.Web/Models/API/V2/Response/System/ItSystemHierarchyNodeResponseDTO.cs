using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;

namespace Presentation.Web.Models.API.V2.Response.System
{
    public class ItSystemHierarchyNodeResponseDTO : RegistrationHierarchyNodeWithActivationStatusResponseDTO
    {
        public bool IsInUse { get; set; }
    }
}