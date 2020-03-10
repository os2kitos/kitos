using System;

namespace Core.DomainModel.Qa.References
{
    public class BrokenLinkInExternalReference : IBrokenLinkWithOrigin<ExternalReference>
    {
        public int Id { get; set; }

        public string ValueOfCheckedUrl { get; set; }

        public BrokenLinkCause Cause { get; set; }

        public int? ErrorResponseCode { get; set; }

        public DateTime ReferenceDateOfLatestLinkChange { get; set; }

        public virtual ExternalReference BrokenReferenceOrigin { get; set; }

        public virtual BrokenExternalReferencesReport ParentReport { get; set; }
    }
}
