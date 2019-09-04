using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public interface IItSystemUsageMigrationService
    {
        Result<ResultStatus, IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublic);

        Result<ResultStatus, string> GetMigrationConflicts(int usageSystemId, int toSystemId);

        void toExecute(string input);
    }
}
