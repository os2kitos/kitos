using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationServiceAdapter
    {
        Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid);
        Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid);
        IEnumerable<CommandPermissionResult> GetCommandPermissions();
        Result<IQueryable<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid, int numberOfItSystems, params IDomainQuery<ItSystem>[] conditions);
    }
}
