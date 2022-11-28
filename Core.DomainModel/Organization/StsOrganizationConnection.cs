using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization.Strategies;

namespace Core.DomainModel.Organization
{

    public interface IExternalOrganizationalHierarchyConnection
    {
        bool Connected { get; }
        public int? SynchronizationDepth { get; }
        IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy();
        bool SubscribeToUpdates { get; }
        StsOrganizationConnectionAddNewLogsResult AddNewLogs(IEnumerable<StsOrganizationChangeLog> newLogs);
        Result<IEnumerable<IExternalConnectionChangelog>, OperationError> GetLastNumberOfChangeLogs(int number = StsOrganizationConnectionConstants.TotalNumberOfLogs);
        DisconnectOrganizationFromOriginResult Disconnect();
        Maybe<OperationError> Subscribe();
        Maybe<OperationError> Unsubscribe();
        Maybe<OperationError> UpdateSynchronizationDepth(int? synchronizationDepth);
    }

    /// <summary>
    /// Determines the properties of the organization's connection to STS Organisation
    /// </summary>
    public class StsOrganizationConnection : Entity, IOwnedByOrganization, IExternalOrganizationalHierarchyConnection
    {
        public StsOrganizationConnection()
        {
            StsOrganizationChangeLogs = new List<StsOrganizationChangeLog>();
        }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public bool Connected { get; set; }
        /// <summary>
        /// Determines the optional synchronization depth used during synchronization from STS Organisation
        /// </summary>
        public int? SynchronizationDepth { get; set; }
        public virtual ICollection<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }


        public bool SubscribeToUpdates { get; set; }

        public DisconnectOrganizationFromOriginResult Disconnect()
        {
            var organizationUnits = Organization.OrgUnits.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
            organizationUnits.ForEach(unit => unit.ConvertToNativeKitosUnit());
            var removedLogs = RemoveAllLogs();

            Connected = false;
            SubscribeToUpdates = false;
            SynchronizationDepth = null;
            return new DisconnectOrganizationFromOriginResult(organizationUnits, removedLogs);
        }

        public IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy()
        {
            return new StsOrganizationalHierarchyUpdateStrategy(Organization);
        }

        public StsOrganizationConnectionAddNewLogsResult AddNewLog(StsOrganizationChangeLog newLog)
        {
            StsOrganizationChangeLogs.Add(newLog);
            var removedLogs = RemoveOldestLogs();

            return new StsOrganizationConnectionAddNewLogsResult(removedLogs);
        }

        public Result<IEnumerable<StsOrganizationChangeLog>, OperationError> GetLastNumberOfChangeLogs(int number = StsOrganizationConnectionConstants.TotalNumberOfLogs)
        {
            if (number <= 0)
            {
                return new OperationError("Number of change logs to get cannot be larger than 0", OperationFailure.BadState);
            }

            return StsOrganizationChangeLogs
                .OrderByDescending(x => x.LogTime)
                .Take(number)
                .ToList();
        }

        private IEnumerable<StsOrganizationChangeLog> RemoveAllLogs()
        {
            var changeLogs = StsOrganizationChangeLogs.ToList();
            foreach (var changeLog in changeLogs)
            {
                StsOrganizationChangeLogs.Remove(changeLog);
            }

            return changeLogs;
        }

        private IEnumerable<StsOrganizationChangeLog> RemoveOldestLogs()
        {
            var logsToRemove = StsOrganizationChangeLogs
                .OrderByDescending(x => x.LogTime)
                .Skip(StsOrganizationConnectionConstants.TotalNumberOfLogs)
                .ToList();

            logsToRemove.ForEach(log => StsOrganizationChangeLogs.Remove(log));

            return logsToRemove;
        }
    }
}
