using Core.DomainModel.ItContract;
using System;
using System.Linq;

namespace Core.DomainServices.Queries.Notifications
{
    public class QueryBySinceNotificationFromDate : IDomainQuery<DomainModel.Advice.Advice>
    {
        private readonly DateTime _since;

        public QueryBySinceNotificationFromDate(DateTime since)
        {
            _since = since;
        }

        public IQueryable<DomainModel.Advice.Advice> Apply(IQueryable<DomainModel.Advice.Advice> source)
        {
            return source.Where(notification => notification.AlarmDate >= _since);
        }
    }
}
