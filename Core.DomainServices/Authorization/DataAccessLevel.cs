namespace Core.DomainServices.Authorization
{
    public class DataAccessLevel
    {
        public CrossOrganizationDataReadAccessLevel CrossOrganizational { get; }
        public OrganizationDataReadAccessLevel CurrentOrganization { get; }

        public DataAccessLevel(CrossOrganizationDataReadAccessLevel crossOrganizational, OrganizationDataReadAccessLevel currentOrganization)
        {
            CrossOrganizational = crossOrganizational;
            CurrentOrganization = currentOrganization;
        }
    }
}
