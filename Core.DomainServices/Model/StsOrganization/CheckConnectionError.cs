namespace Core.DomainServices.Model.StsOrganization
{
    public enum CheckConnectionError
    {
        InvalidCvrOnOrganization = 0,
        MissingServiceAgreement = 1,
        ExistingServiceAgreementIssue = 2,
        Unknown = 3,
        UserContextDoesNotExistOnSystem = 4,
        FailedToLookupOrganizationCompany = 5
    }
}
