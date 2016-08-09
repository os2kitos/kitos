using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal type option.
    /// </summary>
    public class GoalType : Entity, IOptionEntity<Goal>
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }

        public virtual ICollection<Goal> References { get; set; } = new List<Goal>();
    }
}
