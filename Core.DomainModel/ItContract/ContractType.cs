using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class ContractType : IOptionEntity<ItContract>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }
}
