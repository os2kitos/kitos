using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract contract template options.
    /// </summary>
    public class ContractTemplate : Entity, IOptionEntity<ItContract>
    {
        public ContractTemplate()
        {
            References = new List<ItContract>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }
}
