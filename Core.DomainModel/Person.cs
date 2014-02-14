using System.Collections.Generic;

namespace Core.DomainModel
{
    public class Person
    {
        public Person()
        {
            this.ItSystems = new List<ItSystem.ItSystem>();
            this.OwnerOfItProjects = new List<ItProject.ItProject>();
            this.LeaderOfItProjects = new List<ItProject.ItProject>();
            this.LeaderOfPartItProjects = new List<ItProject.ItProject>();
            this.ConsultantOnItProjects = new List<ItProject.ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Municipality_Id { get; set; }

        public virtual Municipality Municipality { get; set; }
        public virtual ICollection<ItSystem.ItSystem> ItSystems { get; set; }
        public virtual ICollection<ItProject.ItProject> OwnerOfItProjects { get; set; }
        public virtual ICollection<ItProject.ItProject> LeaderOfItProjects { get; set; }
        public virtual ICollection<ItProject.ItProject> LeaderOfPartItProjects { get; set; }
        public virtual ICollection<ItProject.ItProject> ConsultantOnItProjects { get; set; }
    }
}