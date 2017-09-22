using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract agreement elements options.
    /// </summary>
    public class AgreementElementType : OptionEntity<ItContract>, IOptionReference<ItContract>
    {
        public virtual ICollection<ItContract> References { get; set; } = new HashSet<ItContract>();
    }
}
