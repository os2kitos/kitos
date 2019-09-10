using System.Linq;
using Core.DomainModel;
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
        private readonly int _activeOrganizationId;
        private readonly bool _hasOrganization;
        private readonly bool _hasAccessModifier;

        public QueryAllByRestrictionCapabilities(CrossOrganizationDataReadAccessLevel crossOrganizationAccess, int activeOrganizationId)
        {
            _crossOrganizationAccess = crossOrganizationAccess;
            _activeOrganizationId = activeOrganizationId;
            _hasOrganization = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            _hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            if (_crossOrganizationAccess == CrossOrganizationDataReadAccessLevel.All)
            {
                return source;
            }

            if (_hasAccessModifier && _crossOrganizationAccess >= CrossOrganizationDataReadAccessLevel.Public)
            {
                if (_hasOrganization)
                {
                    var refinement = QueryFactory.ByPublicAccessOrOrganizationId<T>(_activeOrganizationId);

                    return refinement.Apply(source);
                }
                else
                {
                    var refinement = QueryFactory.ByPublicAccessModifier<T>();

                    return refinement.Apply(source);
                }
            }

            if (_hasOrganization)
            {
                var refinement = QueryFactory.ByOrganizationId<T>(_activeOrganizationId);

                return refinement.Apply(source);
            }

            return source;
        }

        public bool RequiresPostFiltering()
        {
            var hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));

            if (hasAccessModifier && _crossOrganizationAccess >= CrossOrganizationDataReadAccessLevel.Public)
            {
                //Supported by query - shared data is returned and filtered by organization if target has organization reference
                return false;
            }

            var hasOrg = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            var contextAware = typeof(IContextAware).IsAssignableFrom(typeof(T));

            //Unsupported by db query since object does not have org reference but has access modifier and is context aware (belongs in an organization)
            return hasOrg == false && (hasAccessModifier && contextAware);
        }
    }
}
