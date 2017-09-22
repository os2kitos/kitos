using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project goal type option.
    /// </summary>
    public class GoalType : OptionEntity<Goal>, IOptionReference<Goal>
    {
        public virtual ICollection<Goal> References { get; set; } = new HashSet<Goal>();
    }
}
