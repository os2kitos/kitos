using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ProjectCategory : IOptionEntity<ItProject>
    {
        public ProjectCategory()
        {
            References = new List<ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItProject> References { get; set; }
    }
}
