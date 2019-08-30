using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryAllByRestrictionCapabilities<T> : IDomainQuery<T>
    where T : class
    {
        private readonly bool _allowCrossOrganizationAccess;
        private readonly int _organizationId;
        private readonly bool _hasOrganization;
        private readonly bool _hasAccessModifier;

        public QueryAllByRestrictionCapabilities(bool allowCrossOrganizationAccess, int organizationId)
        {
            _allowCrossOrganizationAccess = allowCrossOrganizationAccess;
            _organizationId = organizationId;
            _hasOrganization = typeof(IHasOrganization).IsAssignableFrom(typeof(T));
            _hasAccessModifier = typeof(IHasAccessModifier).IsAssignableFrom(typeof(T));
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            if ((_allowCrossOrganizationAccess || _hasOrganization == false) && _hasAccessModifier)
            {

                var refinement = _hasOrganization
                    ? QueryFactory.ByPublicAccessOrOrganizationId<T>(_organizationId)
                    : QueryFactory.ByPublicAccessModifier<T>();

                return refinement.Apply(source);
            }
            else
            {
                var refinement = QueryFactory.ByOrganizationId<T>(_organizationId);

                return refinement.Apply(source);
            }
        }
    }
}
