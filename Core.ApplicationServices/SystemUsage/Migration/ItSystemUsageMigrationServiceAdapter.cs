using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Shared;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Generic;
using Core.DomainServices.Queries;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public class ItSystemUsageMigrationServiceAdapter : IItSystemUsageMigrationServiceAdapter
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IItSystemUsageMigrationService _systemUsageMigrationService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IOrganizationService _organizationService;

        public ItSystemUsageMigrationServiceAdapter(IEntityIdentityResolver identityResolver,
            IItSystemUsageMigrationService systemUsageMigrationService,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IOrganizationService organizationService)
        {
            _identityResolver = identityResolver;
            _systemUsageMigrationService = systemUsageMigrationService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _organizationService = organizationService;
        }

        public Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid)
        {
            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                .Bind(result => _systemUsageMigrationService.GetSystemUsageMigration(result.usageId, result.systemId))
                .Bind(migration => WithReadAccess(migration.SystemUsage)
                    .Select(_ => migration)
                );
        }

        public Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid)
        {
            var transaction = _transactionManager.Begin();

            return ResolveUsageAndSystemIds(usageUuid, toSystemUuid)
                .Bind(result => _systemUsageMigrationService.ExecuteSystemUsageMigration(result.usageId, result.systemId))
                .Bind(WithModifyAccess)
                .Match(
                    usage =>
                    {
                        transaction.Commit();
                        return Result<ItSystemUsage, OperationError>.Success(usage);
                    },
                    error =>
                    {
                        transaction.Rollback();
                        return error;
                    }
                );
        }

        public Result<IEnumerable<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations,
            params IDomainQuery<ItSystem>[] conditions)
        {
            return _organizationService.GetOrganization(organizationUuid)
                .Bind(organization => _systemUsageMigrationService.GetUnusedItSystemsByOrganizationQuery(organization.Id, numberOfItSystems, getPublicFromOtherOrganizations, conditions))
                .Select<IEnumerable<ItSystem>>(x => x.ToList());
        }

        public IEnumerable<CommandPermissionResult> GetCommandPermissions()
        {
            var commandPermissions = new List<CommandPermissionResult>
            {
                new (CommandPermissionConstraints.UsageMigration.Execute, _systemUsageMigrationService.CanExecuteMigration())
            };

            return commandPermissions;
        }

        private Result<ItSystemUsage, OperationError> WithReadAccess(ItSystemUsage usage)
        {
            return _authorizationContext.AllowReads(usage)
                ? usage
                : new OperationError($"User is not allowed to read It System Usage with uuid: {usage.Uuid}", OperationFailure.Forbidden);
        }

        private Result<ItSystemUsage, OperationError> WithModifyAccess(ItSystemUsage usage)
        {
            return _authorizationContext.AllowReads(usage)
                ? usage
                : new OperationError($"User is not allowed to modify It System Usage with uuid: {usage.Uuid}", OperationFailure.Forbidden);
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
