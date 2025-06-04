using System;
using System.Collections.Generic;

namespace Core.DomainModel.GDPR
{
    public class DprSnapshot : ISnapshot<DataProcessingRegistration>
    {
        public HashSet<Guid> DataProcessorUuids { get; set; }
    }
}
