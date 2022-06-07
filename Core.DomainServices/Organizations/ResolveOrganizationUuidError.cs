namespace Core.DomainServices.Organizations
{
    public enum ResolveOrganizationUuidError
    {
        InvalidCvrOnOrganization,
        FailedToLookupOrganizationCompany,
        FailedToSearchForOrganizationByCompanyUuid,
        DuplicateOrganizationResults,
        FailedToSaveUuidOnKitosOrganization
    }
}
