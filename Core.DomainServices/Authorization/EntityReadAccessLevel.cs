namespace Core.DomainServices.Authorization
{
    public enum EntityReadAccessLevel
    {
        None,
        OrganizationOnly,
        OrganizationAndPublicFromOtherOrganizations,
        All
    }
}
