using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public partial class ProjectType
    {
        public ProjectType()
        {
            this.ItProjectSets = new List<ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ItProject> ItProjectSets { get; set; }
    }
}
