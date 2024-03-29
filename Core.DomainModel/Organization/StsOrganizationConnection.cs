﻿using System;
using System.Linq;
using Core.Abstractions.Types;
using System.Collections.Generic;
using Core.DomainModel.Constants;
using Core.DomainModel.Organization.Strategies;

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// Determines the properties of the organization's connection to STS Organisation
    /// </summary>
    public class StsOrganizationConnection : Entity, IOwnedByOrganization, IExternalOrganizationalHierarchyConnection
    {
        public StsOrganizationConnection()
        {
            StsOrganizationChangeLogs = new List<StsOrganizationChangeLog>();
        }

        public static StsOrganizationConnection Create(Organization organization)
        {
            return new StsOrganizationConnection()
            {
                Organization = organization
            };
        }

        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }
        public bool Connected { get; set; }
        /// <summary>
        /// Determines the optional synchronization depth used during synchronization from STS Organisation
        /// </summary>
        public int? SynchronizationDepth { get; set; }
        /// <summary>
        /// Determines if the organization subscribes to automatic updates from STS Organisation
        /// </summary>
        public bool SubscribeToUpdates { get; set; }

        public OrganizationUnitOrigin Origin => OrganizationUnitOrigin.STS_Organisation;

        /// <summary>
        /// The latest data where changes were checked due to an automatic subscription
        /// This will be null if <see cref="SubscribeToUpdates"/> is false or no automatic check has run yet.
        /// </summary>
        public DateTime? DateOfLatestCheckBySubscription { get; set; }

        public virtual ICollection<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }

        public DisconnectOrganizationFromOriginResult Disconnect()
        {
            var organizationUnits = Organization.OrgUnits.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
            organizationUnits.ForEach(unit => unit.ConvertToNativeKitosUnit());
            var removedLogs = RemoveAllLogs();

            Connected = false;
            SubscribeToUpdates = false;
            SynchronizationDepth = null;
            DateOfLatestCheckBySubscription = null;
			
            return new DisconnectOrganizationFromOriginResult(organizationUnits, removedLogs);
        }

        public void Connect()
        {
            Connected = true;
        }

        public Maybe<OperationError> Subscribe()
        {
            if (!Connected)
                return new OperationError("Organization isn't connected to the sts service", OperationFailure.BadState);

            SubscribeToUpdates = true;
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> Unsubscribe()
        {
            if(!Connected)
                return new OperationError("Organization isn't connected to the sts service", OperationFailure.BadState);

            SubscribeToUpdates = false;
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> UpdateSynchronizationDepth(int? synchronizationDepth)
        {
            if (!Connected)
                return new OperationError("Organization isn't connected to the sts service", OperationFailure.BadState);

            SynchronizationDepth = synchronizationDepth;
            return Maybe<OperationError>.None;
        }

        public void UpdateDateOfLatestUpdateBySubscriptionCheck(DateTime timeStamp)
        {
            DateOfLatestCheckBySubscription = timeStamp;
        }

        public IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy()
        {
            return new StsOrganizationalHierarchyUpdateStrategy(Organization);
        }
		
        public Result<ExternalConnectionAddNewLogsResult, OperationError> AddNewLog(ExternalConnectionAddNewLogInput newLogInput)
        {
            if (newLogInput == null)
            {
                throw new ArgumentNullException(nameof(newLogInput));
            }
            if (!Connected)
            {
                return new OperationError("Organization not connected to the sts organization", OperationFailure.BadState);
            }
            var newLogEntries = newLogInput.Entries.Select(x =>
                new StsOrganizationConsequenceLog
                {
                    Description = x.Description,
                    ExternalUnitUuid = x.Uuid,
                    Name = x.Name,
                    Type = x.Type
                }
            ).ToList();
            var newLog = new StsOrganizationChangeLog
            {
                ResponsibleUserId = newLogInput.ResponsibleUserId,
                ResponsibleType = newLogInput.ResponsibleType,
                LogTime = newLogInput.LogTime,
                Entries = newLogEntries
            };

            StsOrganizationChangeLogs.Add(newLog);
            var removedLogs = RemoveOldestLogs();

            return new ExternalConnectionAddNewLogsResult(removedLogs);
        }

        public Result<IEnumerable<IExternalConnectionChangelog>, OperationError> GetLastNumberOfChangeLogs(int number = ExternalConnectionConstants.TotalNumberOfLogs)
        {
            if (!Connected)
            {
                return new OperationError("Organization not connected to the sts organization", OperationFailure.BadState);
            }
            if (number <= 0)
            {
                return new OperationError("Number of change logs to get cannot be larger than 0", OperationFailure.BadInput);
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
                .Skip(ExternalConnectionConstants.TotalNumberOfLogs)
                .ToList();

            logsToRemove.ForEach(log => StsOrganizationChangeLogs.Remove(log));

            return logsToRemove;
        }
    }
}
