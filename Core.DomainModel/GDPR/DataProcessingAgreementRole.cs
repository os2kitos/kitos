using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingAgreementRole : OptionEntity<DataProcessingAgreementRight>, IRoleEntity, IOptionReference<DataProcessingAgreementRight>
    {
        public bool HasReadAccess { get; set; }
        
        public bool HasWriteAccess { get; set; }

        public virtual ICollection<DataProcessingAgreementRight> References { get; set; }
    }
}
