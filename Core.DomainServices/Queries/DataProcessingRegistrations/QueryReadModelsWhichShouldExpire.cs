using System;
using System.Linq;
using Core.DomainModel.GDPR.Read;
using Core.DomainServices.Queries.Helpers;

namespace Core.DomainServices.Queries.DataProcessingRegistrations
{
    public class QueryReadModelsWhichShouldExpire : IDomainQuery<DataProcessingRegistrationReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldExpire(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<DataProcessingRegistrationReadModel> Apply(IQueryable<DataProcessingRegistrationReadModel> source)
        {
            var currentTime = _currentTime.Date;
            return source.Where(
                x =>
                (
                    x.SourceEntity.MainContract != null &&
                    ItContractIsActiveQueryHelper.CheckIfContractIsExpired(currentTime, x.SourceEntity.MainContract)
                )
            );
        }
    }
}
