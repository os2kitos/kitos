using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public interface IItSystemUsageMigrationService
    {
        Result<OperationResult, IReadOnlyList<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        Result<OperationResult, Model.ItSystemUsage.ItSystemUsageMigration> GetSystemUsageMigration(int usageSystemId, int toSystemId);
        Result<OperationResult, int> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
