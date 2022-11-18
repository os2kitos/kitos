using System.Collections.Generic;
using System.Linq;
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
        public ICollection<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }

        //TODO: https://os2web.atlassian.net/browse/KITOSUDV-3312 adds automatic subscription here
        public DisconnectOrganizationFromOriginResult Disconnect()
        {
            var organizationUnits = Organization.OrgUnits.Where(x => x.Origin == OrganizationUnitOrigin.STS_Organisation).ToList();
            organizationUnits.ForEach(unit => unit.ConvertToNativeKitosUnit());

            Connected = false;
            SynchronizationDepth = null;
            return new DisconnectOrganizationFromOriginResult(organizationUnits);
        }

        public IExternalOrganizationalHierarchyUpdateStrategy GetUpdateStrategy()
        {
            return new StsOrganizationalHierarchyUpdateStrategy(Organization);
        }
    }
}
