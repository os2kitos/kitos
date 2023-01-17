﻿using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DataProcessingBasisForTransferOption : OptionEntity<DataProcessingRegistration>, IOptionReference<DataProcessingRegistration>
    {
        public virtual ICollection<DataProcessingRegistration> References { get; set; }
        public virtual ICollection<SubDataProcessor> SubDataProcessors { get; set; }
    }
}
