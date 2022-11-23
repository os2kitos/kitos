using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.Organization.Strategies;

namespace Core.DomainModel.Organization
{

    /// <summary>
    /// Determines the properties of the organization's connection to STS Organisation
    /// </summary>
    public class StsOrganizationConnection : Entity, IOwnedByOrganization
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

        private const int totalNumberOfLogs = 5;

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

        public StsOrganizationConnectionImportLogResult AddNewLogs(IEnumerable<StsOrganizationChangeLog> newLogs)
        {
            var newLogsList = newLogs.ToList();
            foreach (var newLog in newLogsList)
            {
                StsOrganizationChangeLogs.Add(newLog);
            }

            return RemoveOldestLogs(newLogsList);
        }

        public Result<IEnumerable<StsOrganizationChangeLog>, OperationError> GetLastNumberOfChangeLogs(int number = totalNumberOfLogs)
        {
            if (number <= 0)
            {
                return new OperationError("Number of change logs to get cannot be lower than 0", OperationFailure.BadInput);
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

        private StsOrganizationConnectionImportLogResult RemoveOldestLogs(IEnumerable<StsOrganizationChangeLog> newLogs)
        {
            var logsToRemove = StsOrganizationChangeLogs.OrderByDescending(x => x.LogTime).Skip(totalNumberOfLogs).ToList();
            logsToRemove.ForEach(log => StsOrganizationChangeLogs.Remove(log));

            return new StsOrganizationConnectionImportLogResult(newLogs, logsToRemove);
        }
    }
}
