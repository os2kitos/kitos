
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries.Organization
{
    public class QueryOrganizationByIdsOrSharedAccess : IDomainQuery<DomainModel.Organization.Organization>
    {
        private readonly bool _includePublicOrganizations;
        private readonly IEnumerable<int> _ids;

        public QueryOrganizationByIdsOrSharedAccess(IEnumerable<int> ids, bool includePublicOrganizations)
        {
            _includePublicOrganizations = includePublicOrganizations;
            _ids = ids.ToList().AsReadOnly();
        }

        public IQueryable<DomainModel.Organization.Organization> Apply(IQueryable<DomainModel.Organization.Organization> source)
        {
            return source.Where(organization => _ids.Contains(organization.Id) || (_includePublicOrganizations && organization.AccessModifier == AccessModifier.Public));
        }
    }
}
