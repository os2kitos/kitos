using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationRole : OptionEntity<DataProcessingRegistrationRight>, IRoleEntity, IOptionReference<DataProcessingRegistrationRight>
    {
        public bool HasReadAccess { get; set; }
        
        public bool HasWriteAccess { get; set; }

        public virtual ICollection<DataProcessingRegistrationRight> References { get; set; }
    }
}
