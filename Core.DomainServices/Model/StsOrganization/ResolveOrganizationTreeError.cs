namespace Core.DomainServices.Model.StsOrganization
{
    public enum ResolveOrganizationTreeError
    {
        FailedResolvingUuid,
        FailedSearchingForOrgUnits,
        FailedLoadingOrgUnits,
        FailedToLookupRootUnit
    }
}
