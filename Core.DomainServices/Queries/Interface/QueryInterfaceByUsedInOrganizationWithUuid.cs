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
            return source.Where(x => x.AssociatedSystemRelations.Any(relation => relation.FromSystemUsage.Organization.Uuid == _organizationUuid));
        }
    }
}
