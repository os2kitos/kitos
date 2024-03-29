﻿using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class CriticalityType : OptionEntity<ItContract>, IOptionReference<ItContract>
    {
        public virtual ICollection<ItContract> References { get; set; } = new HashSet<ItContract>();
    }
}
