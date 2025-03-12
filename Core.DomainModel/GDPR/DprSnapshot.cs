using System;
using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DprSnapshot
    {
        public HashSet<Guid> DataProcessorUuids { get; set; }
    }
}
