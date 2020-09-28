using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingOversightOption : OptionEntity<DataProcessingRegistration>, IOptionReference<DataProcessingRegistration>
    {
        public virtual ICollection<DataProcessingRegistration> References { get; set; }
    }
}
