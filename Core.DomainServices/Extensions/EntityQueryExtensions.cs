using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Queries;

namespace Core.DomainServices.Extensions
{
    public static class EntityQueryExtensions
    {
        public static IQueryable<T> ByOrganizationId<T>(this IQueryable<T> result, int organizationId) where T :
            class,
            IHasOrganization
        {
            return new QueryByOrganizationId<T>(organizationId).Apply(result);
        }

        public static IQueryable<T> ByPublicAccessModifier<T>(this IQueryable<T> result) where T :
            class,
            IHasAccessModifier
        {
            return new QueryByAccessModifier<T>(AccessModifier.Public).Apply(result);
        }

        public static IQueryable<T> ByPublicAccessOrOrganizationId<T>(this IQueryable<T> result, int organizationId) where T :
            class,
            IHasAccessModifier,
            IHasOrganization
        {
            return new QueryByPublicAccessOrOrganizationId<T>(organizationId).Apply(result); ;
        }
    }
}
