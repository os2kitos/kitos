using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingCountryOption : OptionEntity<DataProcessingRegistration>, IOptionReference<DataProcessingRegistration>
    {
        public virtual ICollection<DataProcessingRegistration> References { get; set; }
    }
}
