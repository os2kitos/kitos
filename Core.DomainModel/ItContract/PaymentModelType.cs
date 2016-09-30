using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract payment model option.
    /// </summary>
    public class PaymentModelType : OptionEntity<ItContract>, IOptionReference<ItContract>
    {
        public virtual ICollection<ItContract> References { get; set; } = new HashSet<ItContract>();
    }
}
