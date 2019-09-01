using System.Linq;
using Core.DomainModel;
using Core.DomainServices.Authorization;

namespace Core.DomainServices.Queries
{
    public class QueryAllByRestrictionCapabilities<T> : IDomainQuery<T>
    where T : class
    {
        private readonly CrossOrganizationReadAccess _crossOrganizationAccess;
        private readonly int _organizationId;
        private readonly bool _hasOrganization;
        private readonly bool _hasAccessModifier;

        public QueryAllByRestrictionCapabilities(CrossOrganizationReadAccess crossOrganizationAccess, int organizationId)
        {
            _crossOrganizationAccess = crossOrganizationAccess;
            _organizationId = organizationId;
            _hasOrganization = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            _hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            if (_crossOrganizationAccess == CrossOrganizationReadAccess.All)
            {
                return source;
            }
            if (_hasAccessModifier && _crossOrganizationAccess >= CrossOrganizationReadAccess.Public)
            {
                if (_hasOrganization)
                {
                    var refinement = QueryFactory.ByPublicAccessOrOrganizationId<T>(_organizationId);

                    return refinement.Apply(source);
                }
                else
                {

                    var refinement = QueryFactory.ByPublicAccessModifier<T>();

                    return refinement.Apply(source);
                }
            }

            if (_crossOrganizationAccess == CrossOrganizationReadAccess.None && _hasOrganization)
            {
                var refinement = QueryFactory.ByOrganizationId<T>(_organizationId);

                return refinement.Apply(source);
            }

            return source;
        }
    }
}
