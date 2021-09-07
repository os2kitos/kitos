using System;
using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryByResponsibleOrganizationUnitUuid : IDomainQuery<ItContract>
    {
        private readonly Guid _responsibleOrgUnitUuid;

        public QueryByResponsibleOrganizationUnitUuid(Guid responsibleOrgUnitUuid)
        {
            _responsibleOrgUnitUuid = responsibleOrgUnitUuid;
        }

        public IQueryable<ItContract> Apply(IQueryable<ItContract> source)
        {
            return source.Where(contract => contract.ResponsibleOrganizationUnit != null && contract.ResponsibleOrganizationUnit.Uuid == _responsibleOrgUnitUuid);
        }
    }
}
