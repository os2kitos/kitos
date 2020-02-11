namespace Core.DomainModel.Extensions
{
    public static class HasOrganizationExtensions
    {
        public static bool IsInSameOrganizationAs(this IOwnedByOrganization source, IOwnedByOrganization destination)
        {
            return source.OrganizationId == destination.OrganizationId;
        }
    }
}
