namespace Core.DomainServices.Model.StsOrganization
{
    public enum CheckConnectionError
    {
        InvalidCvrOnOrganization,
        MissingServiceAgreement,
        ExistingServiceAgreementIssue,
        UserContextDoesNotExistOnSystem,
        Unknown,
    }
}
