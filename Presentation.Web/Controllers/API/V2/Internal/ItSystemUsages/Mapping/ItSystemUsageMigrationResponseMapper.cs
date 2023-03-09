using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping
{
    public class ItSystemUsageMigrationResponseMapper : IItSystemUsageMigrationResponseMapper
    {
        public ItSystemUsageMigrationV2ResponseDTO MapMigration(ItSystemUsageMigration entity)
        {
            return new ItSystemUsageMigrationV2ResponseDTO
            {
                TargetUsage = entity.SystemUsage.MapIdentityNamePairWithDeactivatedStatusDTO(),
                FromSystem = entity.FromItSystem.MapIdentityNamePairWithDeactivatedStatusDTO(),
                ToSystem = entity.ToItSystem.MapIdentityNamePairWithDeactivatedStatusDTO(),
                AffectedContracts = entity.AffectedContracts.Select(x => x.MapIdentityNamePairDTO()),
                AffectedRelations = entity.AffectedSystemRelations.Select(MapRelationMigration),
                AffectedDataProcessingRegistrations = entity.AffectedDataProcessingRegistrations.Select(x => x.MapIdentityNamePairDTO())
            };
        }

        public IEnumerable<IdentityNamePairWithDeactivatedStatusDTO> MapUnusedSystems(IEnumerable<ItSystem> systems)
        {
            return systems.Select(x => x.MapIdentityNamePairWithDeactivatedStatusDTO()).ToList();
        }

        private static ItSystemUsageRelationMigrationV2ResponseDTO MapRelationMigration(SystemRelation entity)
        {
            return new ItSystemUsageRelationMigrationV2ResponseDTO
            {
                ToSystemUsage = entity.ToSystemUsage?.MapIdentityNamePairWithDeactivatedStatusDTO(),
                FromSystemUsage = entity.FromSystemUsage?.MapIdentityNamePairWithDeactivatedStatusDTO(),
                Description = entity.Description,
                Interface = entity.RelationInterface?.MapIdentityNamePairDTO(),
                FrequencyType = entity.UsageFrequency?.MapIdentityNamePairDTO(),
                Contract = entity.AssociatedContract?.MapIdentityNamePairDTO()
            };
        }
    }
}