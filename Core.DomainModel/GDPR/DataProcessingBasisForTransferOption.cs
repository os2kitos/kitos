using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingBasisForTransferOption : OptionEntity<DataProcessingRegistration>, IOptionReference<DataProcessingRegistration>
    {
        public ICollection<DataProcessingRegistration> References { get; set; }
    }
}
