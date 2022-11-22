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
        public DisconnectOrganizationFromOriginResult Disconnect()
        {
            var organizationUnits = Organization.OrgUnits.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
            organizationUnits.ForEach(unit => unit.ConvertToNativeKitosUnit());
            RemoveAllLogs();

            Connected = false;
            SubscribeToUpdates = false;
            SynchronizationDepth = null;
            return new DisconnectOrganizationFromOriginResult(organizationUnits);
        }

        public IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy()
        {
            return new StsOrganizationalHierarchyUpdateStrategy(Organization);
        }

        public Maybe<StsOrganizationChangeLog> AddNewLog(StsOrganizationChangeLog newLog)
        {
            StsOrganizationChangeLogs.Add(newLog);

            return StsOrganizationChangeLogs.Count <= 5 
                ? Maybe<StsOrganizationChangeLog>.None 
                : RemoveOldestLog();
        }

        public Result<IEnumerable<StsOrganizationChangeLog>, OperationError> GetLastNumberOfChangeLogs(int number)
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

        private void RemoveAllLogs()
        {
            var changeLogs = StsOrganizationChangeLogs.ToList();
            foreach (var changeLog in changeLogs)
            {
                changeLog.RemoveAllConsequences();
                StsOrganizationChangeLogs.Remove(changeLog);
            }
        }

        private StsOrganizationChangeLog RemoveOldestLog()
        {
            var logToRemove = StsOrganizationChangeLogs.OrderBy(x => x.LogTime).FirstOrDefault();
            StsOrganizationChangeLogs.Remove(logToRemove);

            return logToRemove;
        }
    }
}
