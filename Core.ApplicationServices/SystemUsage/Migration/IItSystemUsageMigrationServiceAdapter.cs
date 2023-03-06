using Core.Abstractions.Types;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationServiceAdapter
    {
        Result<ItSystemUsageMigration, OperationError> GetMigration(Guid usageUuid, Guid toSystemUuid);
        Result<ItSystemUsage, OperationError> ExecuteMigration(Guid usageUuid, Guid toSystemUuid);

        Result<IReadOnlyList<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(Guid organizationUuid,
            string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganization);
    }
}
