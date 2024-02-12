namespace Core.DomainServices.Model.StsOrganization
{
    public enum ResolveOrganizationUuidError
    {
        InvalidCvrOnOrganization,
        FailedToLookupOrganizationCompany,
        FailedToSearchForOrganizationByCompanyUuid,
        DuplicateOrganizationResults,
        FailedToSaveUuidOnKitosOrganization,
        MissingServiceAgreement,
        ExistingServiceAgreementIssue,
        UserContextDoesNotExistOnSystem
    }
}
