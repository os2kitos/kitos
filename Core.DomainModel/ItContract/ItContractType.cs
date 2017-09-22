using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract type options.
    /// </summary>
    public class ItContractType : OptionEntity<ItContract>, IOptionReference<ItContract>
    {
        public virtual ICollection<ItContract> References { get; set; } = new HashSet<ItContract>();
    }
}
