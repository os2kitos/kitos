using System.Collections.Generic;
using Core.DomainModel.Advice;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingRegistrationRole : OptionEntity<DataProcessingRegistrationRight>, IRoleEntity, IOptionReference<DataProcessingRegistrationRight>
    {
        public bool HasReadAccess { get; set; }
        
        public bool HasWriteAccess { get; set; }

        public virtual ICollection<DataProcessingRegistrationRight> References { get; set; }

        public virtual ICollection<AdviceUserRelation> AdviceUserRelations { get; set; } = new List<AdviceUserRelation>();
    }
}
