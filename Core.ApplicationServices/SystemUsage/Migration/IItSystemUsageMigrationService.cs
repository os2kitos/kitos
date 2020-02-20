using System.Collections.Generic;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationService
    {
        Result<IReadOnlyList<ItSystem>,OperationFailure> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        Result<ItSystemUsageMigration, OperationFailure> GetSystemUsageMigration(int usageId, int toSystemId);
        Result<ItSystemUsage, OperationFailure> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
