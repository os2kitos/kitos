using Core.Abstractions.Types;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization.Strategies;
using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public interface IExternalOrganizationalHierarchyConnection
    {
        bool Connected { get; }
        public int? SynchronizationDepth { get; }
        IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy();
        bool SubscribeToUpdates { get; }
        StsOrganizationConnectionAddNewLogsResult AddNewLog(StsOrganizationChangeLog newLog);
        Result<IEnumerable<IExternalConnectionChangelog>, OperationError> GetLastNumberOfChangeLogs(int number = StsOrganizationConnectionConstants.TotalNumberOfLogs);
        DisconnectOrganizationFromOriginResult Disconnect();
        void Connect();
        Maybe<OperationError> Subscribe();
        Maybe<OperationError> Unsubscribe();
        Maybe<OperationError> UpdateSynchronizationDepth(int? synchronizationDepth);
    }
}
