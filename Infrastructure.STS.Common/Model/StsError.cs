namespace Infrastructure.STS.Common.Model
{
    public enum StsError
    {
        NotFound,
        BadInput,
        MissingServiceAgreement,
        ExistingServiceAgreementIssue,
        ReceivedUserContextDoesNotExistOnSystem,
        Unknown
    }
}
