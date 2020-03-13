using System;
using System.Collections.Generic;

namespace Core.DomainModel.Qa.References
{
    public class BrokenExternalReferencesReport
    {
        public BrokenExternalReferencesReport()
        {
            BrokenExternalReferences = new List<BrokenLinkInExternalReference>();
            BrokenInterfaceLinks = new List<BrokenLinkInInterface>();
        }

        public int Id { get; set; }

        public DateTime Created { get; set; }

        public virtual ICollection<BrokenLinkInInterface> BrokenInterfaceLinks { get; set; }

        public virtual ICollection<BrokenLinkInExternalReference> BrokenExternalReferences { get; set; }

        public IEnumerable<IBrokenLink> GetBrokenLinks()
        {
            foreach (var brokenLinkInExternalReference in BrokenExternalReferences)
                yield return brokenLinkInExternalReference;

            foreach (var brokenLinkInInterface in BrokenInterfaceLinks)
                yield return brokenLinkInInterface;
        }
    }
}
