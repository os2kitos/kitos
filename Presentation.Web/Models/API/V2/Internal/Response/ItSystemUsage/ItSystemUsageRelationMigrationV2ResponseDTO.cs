using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageRelationMigrationV2ResponseDTO
    {
        public IdentityNamePairWithDeactivatedStatusDTO ToSystem { get; set; }
        public IdentityNamePairWithDeactivatedStatusDTO FromSystem { get; set; }
        public string Description { get; set; }
        public IdentityNamePairResponseDTO Interface { get; set; }
        public IdentityNamePairResponseDTO FrequencyType { get; set; }
        public IdentityNamePairResponseDTO Contract { get; set; }
    }
}