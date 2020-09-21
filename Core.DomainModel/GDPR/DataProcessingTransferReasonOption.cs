using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingTransferReasonOption : OptionEntity<DataProcessingAgreement>, IOptionReference<DataProcessingAgreement>
    {
        public ICollection<DataProcessingAgreement> References { get; set; }
    }
}
