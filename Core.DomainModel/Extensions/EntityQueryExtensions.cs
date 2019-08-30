using System.Linq;

namespace Core.DomainModel.Extensions
{
    public static class EntityQueryExtensions
    {
        public static IQueryable<T> ByOrganizationId<T>(this IQueryable<T> result, int organizationId) where T :
            class,
            IHasOrganization
        {
            return result.Where(x => x.OrganizationId == organizationId);
        }

        public static IQueryable<T> ByPublicAccessModifier<T>(this IQueryable<T> result) where T :
            class,
            IHasAccessModifier
        {
            return result.Where(x => x.AccessModifier == AccessModifier.Public);
        }

        public static IQueryable<T> ByPublicAccessOrOrganizationId<T>(this IQueryable<T> result, int organizationId) where T :
            class,
            IHasAccessModifier,
            IHasOrganization
        {
            return result.Where(x => x.AccessModifier == AccessModifier.Public || x.OrganizationId == organizationId);
        }
    }
}
