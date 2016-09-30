using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract contract template options.
    /// </summary>
    public class ItContractTemplateType : OptionEntity<ItContract>, IOptionReference<ItContract>
    {
        public virtual ICollection<ItContract> References { get; set; } = new HashSet<ItContract>();
    }
}
