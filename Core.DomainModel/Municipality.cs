using System.Collections.Generic;

namespace Core.DomainModel
{
    public class Municipality : IEntity<int>
    {
        public Municipality()
        {
            this.ItProjects = new List<ItProject.ItProject>();
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.ItContracts = new List<ItContract.ItContract>();
            this.Localizations = new List<Localization>();
            this.Users = new List<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual Configuration Configuration { get; set; }
        public virtual ICollection<ItProject.ItProject> ItProjects { get; set; }
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }
        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }
        public virtual ICollection<Localization> Localizations { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
