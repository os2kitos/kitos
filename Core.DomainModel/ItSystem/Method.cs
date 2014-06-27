using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// 
    /// </summary>
    public class Method : Entity, IOptionEntity<ItSystem>
    {
        public Method()
        {
            References = new List<ItSystem>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItSystem> References { get; set; }
    }
}