using System.Collections.Generic;

namespace Core.DomainModel
{
    public enum OrganizationType
    {
        Company,
        Municipality,
        CommunityOfInterests
    }

    public class Organization : HasRightsEntity<Organization, AdminRight, AdminRole>, IHasAccessModifier
    {
        public Organization()
        {
            this.ItProjects = new List<ItProject.ItProject>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.Supplier = new List<ItContract.ItContract>();
            this.ItSystemUsages = new List<ItSystem.ItSystemUsage>();
            this.Contracts = new List<ItContract.ItContract>();
            this.OrgUnits = new List<OrganizationUnit>();
        }

        public string Name { get; set; }
        public OrganizationType Type { get; set; }
        public int? Cvr { get; set; }
        public AccessModifier AccessModifier { get; set; }
        
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }

        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

        //Systems that belongs to this organization (OIO term - think "produced by IRL")
        public virtual ICollection<ItSystem.ItSystem> BelongingSystems { get; set; }
        //KITOS term - which organization was this system created under in KITOS
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }

        /// <summary>
        /// Organization is marked as supplier in these contracts
        /// </summary>
        public virtual ICollection<ItContract.ItContract> Supplier { get; set; }
        public virtual ICollection<ItContract.ItContract> Contracts { get; set; }
        public virtual ICollection<ItSystem.ItSystemUsage> ItSystemUsages { get; set; }

        #region Config and localization

        /* Config and localization */
        public virtual Config Config { get; set; }
        
        #endregion
    }
}
