using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationServiceAdapter : IItSystemUsageMigrationServiceAdapter
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IItSystemUsageMigrationService _systemUsageMigrationService;

        public ItSystemUsageMigrationServiceAdapter(IEntityIdentityResolver identityResolver, 
            IItSystemUsageMigrationService systemUsageMigrationService, 
            IAuthorizationContext authorizationContext)
        {
            _identityResolver = identityResolver;
            _systemUsageMigrationService = systemUsageMigrationService;
            _authorizationContext = authorizationContext;
        }

        public Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid)
        {
            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                    .Bind(result => _systemUsageMigrationService.GetSystemUsageMigration(result.usageId, result.systemId)
                        .Bind
                        (
                            migration => _authorizationContext.AllowReads(migration.SystemUsage) 
                                ? Result<ItSystemUsageMigration, OperationError>.Success(migration) 
                                : new OperationError($"User not allowed to read It System Usage with uuid: {usageUuid}", OperationFailure.Forbidden)
                        )
                    );
        }

        public Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid)
        {
            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                .Bind(result => _systemUsageMigrationService
                    .ExecuteSystemUsageMigration(result.usageId, result.systemId)
                );
        }

        public Result<IReadOnlyList<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid,
            string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganization)
        {
            return _identityResolver.ResolveDbId<Organization>(organizationUuid)
                .Match(id => _systemUsageMigrationService.GetUnusedItSystemsByOrganization(id, nameContent, numberOfItSystems, getPublicFromOtherOrganization), 
                    () => new OperationError($"Organization with uuid: {organizationUuid} was not found", OperationFailure.NotFound)
                );
        }

        private Result<(int usageId, int systemId), OperationError> ResolveUsageAndSystemIds(Guid usageUuid, Guid systemUuid)
        {
            return ResolveUsageId(usageUuid)
                .Bind(usageId => ResolveSystemId(systemUuid)
                    .Select(systemId => (usageId, systemId))
                );
        }

        private Result<int, OperationError> ResolveUsageId(Guid usageUuid)
        {
            return _identityResolver.ResolveDbId<ItSystemUsage>(usageUuid)
                .Match<Result<int, OperationError>>(id => id, () => new OperationError($"ItSystemUsage with uuid: {usageUuid} was not found", OperationFailure.NotFound));
        }

        private Result<int, OperationError> ResolveSystemId(Guid systemUuid)
        {
            return _identityResolver.ResolveDbId<ItSystem>(systemUuid)
                .Match<Result<int, OperationError>>(id => id, () => new OperationError($"ItSystem with uuid: {systemUuid} was not found", OperationFailure.NotFound));
        }
    }
}
