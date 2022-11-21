using System;
using System.Linq;
using Core.DomainModel.Organization.Strategies;

namespace Core.DomainModel.Organization
{

    /// <summary>
    /// Determines the properties of the organization's connection to STS Organisation
    /// </summary>
    public class StsOrganizationConnection : Entity, IOwnedByOrganization
    {
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
        /// <summary>
        /// The latest data where changes were checked due to an automatic subscription
        /// This will be null if <see cref="SubscribeToUpdates"/> is false or no automatic check has run yet.
        /// </summary>
        public DateTime? DateOfLatestCheckBySubscription { get; set; }
        //TODO https://os2web.atlassian.net/browse/KITOSUDV-3317 adds the change logs here
        public DisconnectOrganizationFromOriginResult Disconnect()
        {
            var organizationUnits = Organization.OrgUnits.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
            organizationUnits.ForEach(unit => unit.ConvertToNativeKitosUnit());

            Connected = false;
            SubscribeToUpdates = false;
            SynchronizationDepth = null;
            DateOfLatestCheckBySubscription = null;
            return new DisconnectOrganizationFromOriginResult(organizationUnits);
        }

        public IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy()
        {
            return new StsOrganizationalHierarchyUpdateStrategy(Organization);
        }
    }
}
