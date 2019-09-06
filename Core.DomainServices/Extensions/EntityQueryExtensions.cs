using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Authorization;
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

        public static IQueryable<T> ByOrganizationId<T>(this IQueryable<T> result, int organizationId, OrganizationDataReadAccessLevel accessLevel) where T :
            class,
            IHasOrganization
        {
            return QueryFactory.ByOrganizationId<T>(organizationId, accessLevel).Apply(result);
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

        public static IQueryable<T> ByOrganizationDataAndPublicDataFromOtherOrganizations<T>(
            this IQueryable<T> result,
            int organizationId,
            OrganizationDataReadAccessLevel organizationAccessLevel,
            CrossOrganizationDataReadAccessLevel crossOrganizationDataReadAccessLevel)
            where T : class, IHasAccessModifier, IHasOrganization
        {
            var domainQueries = new List<IDomainQuery<T>>();

            /****************/
            /***MAIN QUERY***/
            /****************/
            if (organizationAccessLevel >= OrganizationDataReadAccessLevel.Public)
            {
                if (crossOrganizationDataReadAccessLevel >= CrossOrganizationDataReadAccessLevel.Public)
                {
                    domainQueries.Add(new QueryByPublicAccessOrOrganizationId<T>(organizationId));
                }
                else
                {
                    domainQueries.Add(new QueryByOrganizationId<T>(organizationId));
                }

                //Refine to public data only for the organization in question
                if (organizationAccessLevel == OrganizationDataReadAccessLevel.Public)
                {
                    domainQueries.Add(new QueryByAccessModifier<T>(AccessModifier.Public));
                }
            }
            else
            {
                domainQueries.Add(new RejectAllResultsQuery<T>());
            }

            return new IntersectionQuery<T>(domainQueries).Apply(result);
        }

        public static IQueryable<T> ExceptEntitiesWithIds<T>(this IQueryable<T> result, IReadOnlyList<int> exceptIds) where T :
            Entity
        {
            return new QueryExceptEntitiesWithIds<T>(exceptIds).Apply(result);
        }

        public static IQueryable<T> ByPartOfName<T>(this IQueryable<T> result, string nameContent) where T :
            class,
            IHasName
        {
            return new QueryByPartOfName<T>(nameContent).Apply(result);
        }
    }
}
