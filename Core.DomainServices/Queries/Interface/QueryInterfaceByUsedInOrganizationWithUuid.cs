using System;
using System.Linq;
using Core.DomainModel.ItSystem;

namespace Core.DomainServices.Queries.Interface
{
    public class QueryInterfaceByUsedInOrganizationWithUuid : IDomainQuery<ItInterface>
    {
        private readonly Guid _organizationUuid;

        public QueryInterfaceByUsedInOrganizationWithUuid(Guid organizationUuid)
        {
            _organizationUuid = organizationUuid;
        }

        public IQueryable<ItInterface> Apply(IQueryable<ItInterface> source)
        {
            return source.Where(x => x.ExhibitedBy.ItSystem.Usages.Any(usage => usage.Organization.Uuid == _organizationUuid));
        }
    }
}
