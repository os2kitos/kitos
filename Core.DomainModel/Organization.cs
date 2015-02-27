using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.DomainModel
{
    public enum OrganizationType
    {
        Company,
        Municipality,
        CommunityOfInterests
    }
    
    /// <summary>
    /// Represents an Organization (such as a municipality, or a company).
    /// Holds local configuration and admin roles, as well as collections of
    /// ItSystems, ItProjects, etc that was created in this organization.
    /// </summary>
    public class Organization : HasRightsEntity<Organization, AdminRight, AdminRole>, IHasAccessModifier
    {
        public Organization()
        {
            this.ItProjects = new List<ItProject.ItProject>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.Supplier = new List<ItContract.ItContract>();
            this.ItSystemUsages = new List<ItSystemUsage.ItSystemUsage>();
            this.Contracts = new List<ItContract.ItContract>();
            this.OrgUnits = new List<OrganizationUnit>();
        }

        public string Name { get; set; }
        public OrganizationType Type { get; set; }
        /// <summary>
        /// Cvr number
        /// </summary>
        /// <remarks>
        /// This is a string instead of int because it's much easier to do a partial search on strings
        /// </remarks>
        public string Cvr { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public Guid? Uuid { get; set; }
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }

        /// <summary>
        /// ItProjects created inside this organization
        /// </summary>
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

        //Systems that belongs to this organization (OIO term - think "produced by IRL")
        public virtual ICollection<ItSystem.ItSystem> BelongingSystems { get; set; }
        //KITOS term - which organization was this system created under in KITOS
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }

        /// <summary>
        /// Organization is marked as supplier in these contracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> Supplier { get; set; }

        /// <summary>
        /// Contracts created inside the organization
        /// </summary>
        public virtual ICollection<ItContract.ItContract> Contracts { get; set; }

        /// <summary>
        /// Local usages of IT systems within this organization
        /// </summary>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> ItSystemUsages { get; set; }
        
        /// <summary>
        /// Local configuration of KITOS
        /// </summary>
        public virtual Config Config { get; set; }

        /// <summary>
        /// Get the level-0 organization unit, which by convention is named represently
        /// </summary>
        /// <returns></returns>
        public OrganizationUnit GetRoot()
        {
            return OrgUnits.FirstOrDefault(u => u.Parent == null);
        }

        public override bool HasUserWriteAccess(User user, int organizationId)
        {
            // check that object belongs to the requwested organization context
            if (Id != organizationId && !user.IsGlobalAdmin)
                return false;
            return base.HasUserWriteAccess(user, organizationId);
        }
    }
}
