using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjectCategory : IEntity<int>
    {
        public ProjectCategory()
        {
            this.ItProjects = new List<ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }

        public virtual ICollection<ItProject> ItProjects { get; set; }
    }
}
