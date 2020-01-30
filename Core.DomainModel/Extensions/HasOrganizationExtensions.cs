namespace Core.DomainModel.Extensions
{
    public static class HasOrganizationExtensions
    {
        public static bool IsInSameOrganizationAs(this IHasOrganization source, IHasOrganization destination)
        {
            return source.OrganizationId == destination.OrganizationId;
        }
    }
}
