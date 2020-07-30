using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.Result;
using Core.DomainServices.Authorization;

namespace Core.DomainServices.Queries
{
    /// <summary>
    /// Realizes the generic query that queries all available data with respect to the active context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryAllByRestrictionCapabilities<T> : IDomainQuery<T>
    where T : class
    {
        private readonly CrossOrganizationDataReadAccessLevel _crossOrganizationAccess;
        private readonly IEnumerable<int> _organizationIds;
        private readonly bool _hasOrganization;
        private readonly bool _hasAccessModifier;

        public QueryAllByRestrictionCapabilities(CrossOrganizationDataReadAccessLevel crossOrganizationAccess, IEnumerable<int> organizationIds)
        {
            _crossOrganizationAccess = crossOrganizationAccess;
            _organizationIds = organizationIds;
            _hasOrganization = typeof(IOwnedByOrganization).IsAssignableFrom(typeof(T));
            _hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            var refine = Maybe<IDomainQuery<T>>.None;

            if (_crossOrganizationAccess < CrossOrganizationDataReadAccessLevel.All)
            {
                if (_hasAccessModifier && _crossOrganizationAccess >= CrossOrganizationDataReadAccessLevel.Public)
                {
                    refine = Maybe<IDomainQuery<T>>
                        .Some(_hasOrganization
                            ? QueryFactory.ByPublicAccessOrOrganizationIds<T>(_organizationIds)
                            : QueryFactory.ByPublicAccessModifier<T>()
                        );
                }
                else if (_hasOrganization)
                {
                    refine = Maybe<IDomainQuery<T>>.Some(QueryFactory.ByOrganizationIds<T>(_organizationIds));
                }
            }

            return refine
                .Select(r => r.Apply(source))
                .GetValueOrFallback(source);
        }
    }
}
