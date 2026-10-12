using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageMigrationV2ResponseDTO
    {
        public required IdentityNamePairWithDeactivatedStatusDTO TargetUsage { get; set; }
        public required IdentityNamePairWithDeactivatedStatusDTO FromSystem { get; set; }
        public required IdentityNamePairWithDeactivatedStatusDTO ToSystem { get; set; }
        public required IEnumerable<IdentityNamePairResponseDTO> AffectedContracts { get; set; }
        public required IEnumerable<ItSystemUsageRelationMigrationV2ResponseDTO> AffectedRelations { get; set; }
        public required IEnumerable<IdentityNamePairResponseDTO> AffectedDataProcessingRegistrations { get; set; }
    }
}