﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItSystem
{
    public class Method : IOptionEntity<Interface>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<Interface> References { get; set; }
    }
}