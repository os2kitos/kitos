using System.Collections.Generic;

namespace Core.DomainModel.ItProject
{
    /// <summary>
    /// It project type option.
    /// </summary>
    public class ItProjectType : OptionEntity<ItProject>, IOptionReference<ItProject>
    {
        public virtual ICollection<ItProject> References { get; set; } = new HashSet<ItProject>();
    }
}
