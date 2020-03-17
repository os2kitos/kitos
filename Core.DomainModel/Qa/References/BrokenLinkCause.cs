namespace Core.DomainModel.Qa.References
{
    public enum BrokenLinkCause
    {
        InvalidUrl = 0,
        DnsLookupFailed = 1,
        ErrorResponse = 2,
        CommunicationError = 3
    }
}
