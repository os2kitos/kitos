using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationServiceAdapter
    {
        Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid);
        Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid);
        Result<IEnumerable<CommandPermissionResult>, OperationError> GetCommandPermission(Guid usageUuid);
        Result<IEnumerable<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations,
            params IDomainQuery<ItSystem>[] conditions);
    }
}
