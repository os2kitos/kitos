using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjectType
    {
        public ProjectType()
        {
            this.ItProjects = new List<ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ItProject> ItProjects { get; set; }
    }
}
