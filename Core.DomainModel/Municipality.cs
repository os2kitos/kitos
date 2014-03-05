using System.Collections.Generic;
using Core.DomainModel.ItProject;

namespace Core.DomainModel
{
    public class Municipality : IEntity<int>
    {
        public Municipality()
        {
            this.ItProjects = new List<ItProject.ItProject>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.ItContracts = new List<ItContract.ItContract>();
            this.Users = new List<User>();
            this.ProjectPhaseLocales = new List<ProjPhaseLocale>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        
        public virtual Config Config { get; set; }
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }
        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }
        //public virtual Localization Localization { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<ProjPhaseLocale> ProjectPhaseLocales { get; set; }
        public virtual ICollection<ExtRefTypeLocale> ExtRefTypeLocales { get; set; }

        public virtual ItSupportConfig ItSupportConfig { get; set; }
        public virtual ItProjectConfig ItProjectConfig { get; set; }
        public virtual ItSystemConfig ItSystemConfig { get; set; }
        public virtual ItContractConfig ItContractConfig { get; set; }

    }
}
