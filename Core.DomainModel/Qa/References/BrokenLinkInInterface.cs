using System;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.Qa.References
{
    public class BrokenLinkInInterface : IBrokenLinkWithOrigin<ItInterface>
    {
        public int Id { get; set; }

        public string ValueOfCheckedUrl { get; set; }

        public BrokenLinkCause Cause { get; set; }

        public DateTime ReferenceDateOfLatestLinkChange { get; set; }

        public virtual ItInterface BrokenReferenceOrigin { get; set; }

        public virtual BrokenExternalReferencesReport ParentReport { get; set; }
    }
}
