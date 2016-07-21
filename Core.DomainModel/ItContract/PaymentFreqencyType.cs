using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract payment freqency option.
    /// </summary>
    public class PaymentFreqencyType : Entity, IOptionEntity<ItContract>
    {
        public PaymentFreqencyType()
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
