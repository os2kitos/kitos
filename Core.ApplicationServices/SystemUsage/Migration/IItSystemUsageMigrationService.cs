using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationService
    {
        Result<IReadOnlyList<ItSystem>, OperationError> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        Result<ItSystemUsageMigration, OperationError> GetSystemUsageMigration(int usageId, int toSystemId);
        Result<ItSystemUsage, OperationError> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
