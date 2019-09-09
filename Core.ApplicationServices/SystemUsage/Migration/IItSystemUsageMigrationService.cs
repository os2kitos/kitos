using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationService
    {
        Result<OperationResult, IReadOnlyList<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        Result<OperationResult, ItSystemUsageMigration> GetSystemUsageMigration(int usageSystemId, int toSystemId);
        Result<OperationResult, int> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
