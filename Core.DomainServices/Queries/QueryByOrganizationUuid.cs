using Core.DomainModel;
using System;
using System.Linq;

namespace Core.DomainServices.Queries
{
    public class QueryByOrganizationUuid<T> : IDomainQuery<T>
        where T : class, IOwnedByOrganization
    {
        private readonly Guid _organizationUuid;

        public QueryByOrganizationUuid(Guid organizationUuid)
        {
            _organizationUuid = organizationUuid;
        }

        public IQueryable<T> Apply(IQueryable<T> source)
        {
            return source.Where(x => x.Organization != null && x.Organization.Uuid == _organizationUuid);
        }
    }
}
