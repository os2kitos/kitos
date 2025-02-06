using System;
using System.Linq;
using Core.DomainModel;

namespace Core.DomainServices.Queries
{
    public class QueryByPublicAccessOrOrganizationUuid<T> : IDomainQuery<T>
        where T : class, IHasAccessModifier, IOwnedByOrganization
    {
        private readonly Guid _organizationUuid; 
        public QueryByPublicAccessOrOrganizationUuid(Guid organizationUuid) { _organizationUuid = organizationUuid; } 
        public IQueryable<T> Apply(IQueryable<T> source) { return source.Where(x => x.AccessModifier == AccessModifier.Public || x.Organization.Uuid == _organizationUuid); }
    }
}