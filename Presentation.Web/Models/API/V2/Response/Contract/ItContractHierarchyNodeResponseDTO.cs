using Presentation.Web.Models.API.V2.Response.Generic.Hierarchy;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ItContractHierarchyNodeResponseDTO : RegistrationHierarchyNodeWithActivationStatusResponseDTO
    {
        public bool RequireValidParent { get; set; }
    }
}