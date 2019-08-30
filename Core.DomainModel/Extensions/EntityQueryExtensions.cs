using System.Linq;

namespace Core.DomainModel.Extensions
{
    public static class EntityQueryExtensions
    {
        public static IQueryable<T> ByOrganizationId<T>(this IQueryable<T> result, int organizationId) where T : IHasOrganization
        {
            return result.Where(x => x.OrganizationId == organizationId);
        }

        public static IQueryable<T> ByPublicAccessModifier<T>(this IQueryable<T> result) where T : IHasAccessModifier
        {
            return result.Where(x => x.AccessModifier == AccessModifier.Public);
        }

        public static IQueryable<T> ByPublicAccessOrOrganizationId<T>(this IQueryable<T> result, int organizationId) where T : IHasAccessModifier, IHasOrganization
        {
            return result.Where(x => x.AccessModifier == AccessModifier.Public || x.OrganizationId == organizationId);
        }
    }
}
