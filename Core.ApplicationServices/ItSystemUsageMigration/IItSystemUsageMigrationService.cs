using System.Collections.Generic;
using Core.ApplicationServices.Model;
using Core.ApplicationServices.Model.Result;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public interface IItSystemUsageMigrationService
    {
        Result<OperationResult, IReadOnlyList<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);

        Result<OperationResult, string> GetMigrationConflicts(int usageSystemId, int toSystemId);

        void toExecute(string input);
    }
}
