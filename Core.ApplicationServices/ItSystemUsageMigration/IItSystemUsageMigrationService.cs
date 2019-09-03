using System.Collections.Generic;
using Core.DomainModel;
using Core.DomainModel.ItSystem;

namespace Core.ApplicationServices.ItSystemUsageMigration
{
    public interface IItSystemUsageMigrationService
    {
        Return<IEnumerable<ItSystem>> GetUnusedItSystemsByOrganization(int organizationId, string q, int limit);

        Return<string> GetMigrationConflicts(int fromSystemId, int toSystemId);

        void toExecute(string input);
    }
}
