using System.Linq;

namespace Core.DomainServices.Queries.ItSystem
{
    public class QueryByNumberOfUsages : IDomainQuery<DomainModel.ItSystem.ItSystem>
    {
        private readonly int _greaterThanOrEqualTo;

        public QueryByNumberOfUsages(int greaterThanOrEqualTo)
        {
            _greaterThanOrEqualTo = greaterThanOrEqualTo;
        }

        public IQueryable<DomainModel.ItSystem.ItSystem> Apply(IQueryable<DomainModel.ItSystem.ItSystem> itSystems)
        {
            return itSystems.Where(x => x.Usages.Count >= _greaterThanOrEqualTo);
        }
    }
}
