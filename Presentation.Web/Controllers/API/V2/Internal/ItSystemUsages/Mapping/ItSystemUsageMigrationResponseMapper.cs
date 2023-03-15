using System.Collections.Generic;
using System.Linq;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;

namespace Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping
{
    public class ItSystemUsageMigrationResponseMapper : IItSystemUsageMigrationResponseMapper
    {
        private readonly ICommandPermissionsResponseMapper _commandPermissionsResponseMapper;

        public ItSystemUsageMigrationResponseMapper(ICommandPermissionsResponseMapper commandPermissionsResponseMapper)
        {
            _commandPermissionsResponseMapper = commandPermissionsResponseMapper;
        }

        public ItSystemUsageMigrationV2ResponseDTO MapMigration(ItSystemUsageMigration entity)
        {
            return new ItSystemUsageMigrationV2ResponseDTO
            {
                TargetUsage = entity.SystemUsage.MapIdentityNamePairWithDeactivatedStatusDTO(),
                FromSystem = entity.FromItSystem.MapIdentityNamePairWithDeactivatedStatusDTO(),
                ToSystem = entity.ToItSystem.MapIdentityNamePairWithDeactivatedStatusDTO(),
                AffectedContracts = entity.AffectedContracts.Select(x => x.MapIdentityNamePairDTO()).ToList(),
                AffectedRelations = entity.AffectedSystemRelations.Select(MapRelationMigration).ToList(),
                AffectedDataProcessingRegistrations = entity.AffectedDataProcessingRegistrations.Select(x => x.MapIdentityNamePairDTO()).ToList()
            };
        }

        public ItSystemUsageMigrationPermissionsResponseDTO MapCommandPermissions(IEnumerable<CommandPermissionResult> permissionResult)
        {
            return new ItSystemUsageMigrationPermissionsResponseDTO(permissionResult.Select(_commandPermissionsResponseMapper.MapCommandPermission).ToList());
        }

        private static ItSystemUsageRelationMigrationV2ResponseDTO MapRelationMigration(SystemRelation entity)
        {
            return new ItSystemUsageRelationMigrationV2ResponseDTO
            {
                ToSystem = entity.ToSystemUsage?.ItSystem?.MapIdentityNamePairWithDeactivatedStatusDTO(),
                FromSystem = entity.FromSystemUsage?.ItSystem?.MapIdentityNamePairWithDeactivatedStatusDTO(),
                Description = entity.Description,
                Interface = entity.RelationInterface?.MapIdentityNamePairDTO(),
                FrequencyType = entity.UsageFrequency?.MapIdentityNamePairDTO(),
                Contract = entity.AssociatedContract?.MapIdentityNamePairDTO()
            };
        }
    }
}