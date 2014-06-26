﻿using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class ItContractRole : Entity, IRoleEntity<ItContractRight>
    {
        public ItContractRole()
        {
            HasReadAccess = true;
            References = new List<ItContractRight>();
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContractRight> References { get; set; }

        public virtual ICollection<Advice> ReceiverFor { get; set; }
        public virtual ICollection<Advice> CarbonCopyReceiverFor { get; set; }
    }
}
