namespace Core.DomainServices.Authorization
{
    public enum EntityReadAccessLevel
    {
        None,
        OrganizationOnly,
        OrganizationAndRightsHolderAccess,
        OrganizationAndPublicFromOtherOrganizations,
        All
    }
}
