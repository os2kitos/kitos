using System.Linq;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryByActiveNotification : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly bool _active;

        public QueryByActiveNotification(bool active)
        {
            _active = active;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.IsActive == _active);
        }
    }
}
