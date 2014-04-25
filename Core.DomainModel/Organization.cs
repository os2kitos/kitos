using System.Collections.Generic;
using Core.DomainModel.ItProject;

namespace Core.DomainModel
{
    public enum OrganizationType
    {
        Company,
        Municipality,
        CommunityOfInterests
    }

    public class Organization : IEntity<int>
    {
        public Organization()
        {
            this.ItProjects = new List<ItProject.ItProject>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.ItContracts = new List<ItContract.ItContract>();
            this.ItSystemUsages = new List<ItSystem.ItSystemUsage>();

            this.ProjectPhaseLocales = new List<ProjPhaseLocale>();
            this.OrgUnits = new List<OrganizationUnit>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public OrganizationType? Type { get; set; }
        public int? Cvr { get; set; }
        
        public virtual ICollection<OrganizationUnit> OrgUnits { get; set; }

        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }

        //Systems that belongs to this organization (OIO term - think "produced by IRL")
        public virtual ICollection<ItSystem.ItSystem> BelongingSystems { get; set; }
        //KITOS term - which organization was this system created under in KITOS
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }

        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }
        public virtual ICollection<AdminRight> AdminRights { get; set; }
        public virtual ICollection<ItSystem.ItSystemUsage> ItSystemUsages { get; set; }

        #region Config and localization

        /* Config and localization */
        public virtual Config Config { get; set; }
        public virtual ICollection<ProjPhaseLocale> ProjectPhaseLocales { get; set; }
        public virtual ICollection<ExtRefTypeLocale> ExtRefTypeLocales { get; set; }

        #endregion



    }
}
