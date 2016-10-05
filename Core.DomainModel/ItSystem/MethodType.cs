﻿using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class MethodType : OptionEntity<ItInterface>, IOptionReference<ItInterface>
    {
        public virtual ICollection<ItInterface> References { get; set; } = new HashSet<ItInterface>();
    }
}
