using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingCountryOption : OptionEntity<DataProcessingAgreement>, IOptionReference<DataProcessingAgreement>
    {
        public ICollection<DataProcessingAgreement> References { get; set; }
    }
}
