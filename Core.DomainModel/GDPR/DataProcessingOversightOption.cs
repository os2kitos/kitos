using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingOversightOption : OptionEntity<DataProcessingRegistration>, IOptionReference<DataProcessingRegistration>
    {
        public ICollection<DataProcessingRegistration> References { get; set; }
    }
}
