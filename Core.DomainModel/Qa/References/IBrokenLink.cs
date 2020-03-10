using System;

namespace Core.DomainModel.Qa.References
{
    public interface IBrokenLink
    {
        int Id { get; set; }

        string ValueOfCheckedUrl { get; set; }

        BrokenLinkCause Cause { get; set; }

        DateTime ReferenceDateOfLatestLinkChange { get; set; }
    }
}
