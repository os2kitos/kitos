using System;
using System.Collections.Generic;

namespace Core.DomainModel.Qa.References
{
    public class BrokenExternalReferencesReport
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public virtual ICollection<BrokenLinkInInterface> BrokenInterfaceLinks { get; set; }

        public virtual ICollection<BrokenLinkInExternalReference> BrokenExternalReferences { get; set; }
    }
}
