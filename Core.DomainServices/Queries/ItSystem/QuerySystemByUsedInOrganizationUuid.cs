using System;
using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QuerySystemByUsedInOrganizationUuid : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly Guid _organizationUuid;

        public QuerySystemByUsedInOrganizationUuid(Guid organizationUuid)
        {
            _organizationUuid = organizationUuid;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> source)
        {
            return source.Where(x => x.Usages.Any(usage => usage.Organization.Uuid == _organizationUuid));
        }
    }
}
