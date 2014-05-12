using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    public class GoalType : IOptionEntity<Goal>
    {
        public GoalType()
        {
            this.References = new List<Goal>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<Goal> References { get; set; }
    }
}
