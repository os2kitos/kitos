using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Queries;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationService
    {
        Result<IQueryable<ItSystem>, OperationError> GetUnusedItSystemsByOrganizationQuery(
            int organizationId,
            int numberOfItSystems,
            bool getPublicFromOtherOrganizations,
            params IDomainQuery<ItSystem>[] conditions);

        Result<IReadOnlyList<ItSystem>, OperationError> GetUnusedItSystemsByOrganizationByName(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        Result<ItSystemUsageMigration, OperationError> GetSystemUsageMigration(int usageId, int toSystemId);
        Result<ItSystemUsage, OperationError> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
