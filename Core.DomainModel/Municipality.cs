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
        }

        public int Id { get; set; }
        public string Name { get; set; }
        
        public virtual Configuration Configuration { get; set; }
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }
        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }
        //public virtual Localization Localization { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ProjectPhaseLocale ProjectPhaseLocale { get; set; }
    }
}
