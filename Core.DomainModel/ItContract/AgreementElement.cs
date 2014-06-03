using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class AgreementElement : Entity, IOptionEntity<ItContract>
    {
        public AgreementElement()
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
