using System.Collections.Generic;
using Core.ApplicationServices.Model.Result;
using Core.ApplicationServices.Model.SystemUsage.Migration;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.SystemUsage.Migration
{
    public interface IItSystemUsageMigrationService
    {
        TwoTrackResult<IReadOnlyList<ItSystem>,OperationFailure> GetUnusedItSystemsByOrganization(int organizationId, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations);
        TwoTrackResult<ItSystemUsageMigration, OperationFailure> GetSystemUsageMigration(int usageId, int toSystemId);
        TwoTrackResult<ItSystemUsage, OperationFailure> ExecuteSystemUsageMigration(int usageSystemId, int toSystemId);
        bool CanExecuteMigration();
    }
}
