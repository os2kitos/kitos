using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage
{
    public class ItSystemUsageMigrationV2ResponseDTO
    {
        public IdentityNamePairWithDeactivatedStatusDTO TargetUsage { get; set; }
        public IdentityNamePairWithDeactivatedStatusDTO FromSystem { get; set; }
        public IdentityNamePairWithDeactivatedStatusDTO ToSystem { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> AffectedContracts { get; set; }
        public IEnumerable<ItSystemUsageRelationMigrationV2ResponseDTO> AffectedRelations { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> AffectedDataProcessingRegistrations { get; set; }
    }
}