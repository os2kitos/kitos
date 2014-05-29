using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class ItProjectType : Entity, IOptionEntity<ItProject>
    {
        public ItProjectType()
        {
            this.References = new List<ItProject>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<ItProject> References { get; set; }
    }
}
