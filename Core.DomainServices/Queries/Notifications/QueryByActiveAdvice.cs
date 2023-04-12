using System.Linq;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByActiveAdvice : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly bool _active;

        public QueryByActiveAdvice(bool active)
        {
            _active = active;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.IsActive == _active);
        }
    }
}
