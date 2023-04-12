using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Generic;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Shared;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationServiceAdapter : IItSystemUsageMigrationServiceAdapter
    {
        private readonly IEntityIdMapper _identityResolver;
        private readonly IItSystemUsageMigrationService _systemUsageMigrationService;
        private readonly IOrganizationService _organizationService;

        public ItSystemUsageMigrationServiceAdapter(IEntityIdMapper identityResolver,
            IItSystemUsageMigrationService systemUsageMigrationService,
            IOrganizationService organizationService)
        {
            _identityResolver = identityResolver;
            _systemUsageMigrationService = systemUsageMigrationService;
            _organizationService = organizationService;
        }

        public Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid)
        {
            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                .Bind(result => _systemUsageMigrationService.GetSystemUsageMigration(result.usageId, result.systemId));
        }

        public Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid)
        {
            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                .Bind(result =>
                    _systemUsageMigrationService.ExecuteSystemUsageMigration(result.usageId, result.systemId));
        }

        public Result<IQueryable<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid,
            int numberOfItSystems,
            params IDomainQuery<ItSystem>[] conditions)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Bind(organization => _systemUsageMigrationService.GetUnusedItSystemsByOrganizationQuery(organization.Id, numberOfItSystems, true, conditions));
        }

        public IEnumerable<CommandPermissionResult> GetCommandPermissions()
        {
            var commandPermissions = new List<CommandPermissionResult>
            {
                new (CommandPermissionCommandIds.UsageMigration.Execute, _systemUsageMigrationService.CanExecuteMigration())
            };

            return commandPermissions;
        }

        private Result<(int usageId, int systemId), OperationError> ResolveUsageAndSystemIds(Guid usageUuid, Guid systemUuid)
        {
            return _identityResolver.Map<ItSystemUsage>(usageUuid)
                .Bind(usageId => _identityResolver.Map<ItSystem>(systemUuid)
                    .Select(systemId => (usageId, systemId))
                );
        }
    }
}
