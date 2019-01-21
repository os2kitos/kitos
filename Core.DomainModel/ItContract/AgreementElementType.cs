using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    /// <summary>
    /// It contract agreement elements options.
    /// </summary>
    public class AgreementElementType : OptionEntity<ItContract>, IOptionReference<ItContractAgreementElementTypes>
    {
        public virtual ICollection<ItContractAgreementElementTypes> References { get; set; } = new HashSet<ItContractAgreementElementTypes>();
    }
}
