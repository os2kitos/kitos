using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryReadModelsWhichShouldExpire : IDomainQuery<ItSystemUsageOverviewReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldExpire(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<ItSystemUsageOverviewReadModel> Apply(IQueryable<ItSystemUsageOverviewReadModel> source)
        {
            var currentTime = _currentTime.Date;

            return source.Where(

                x =>
                    // All currently set as active in the read model
                    x.ActiveAccordingToValidityPeriod &&
                    // Expiration data defined
                    x.SourceEntity.ExpirationDate != null &&
                    // Expiration date has passed
                    x.SourceEntity.ExpirationDate < currentTime &&
                    // Main Contract is inactive
                    x.SourceEntity.MainContract != null && 
                    x.SourceEntity.MainContract.ItContract.IsActive == false
            );
        }
    }
}
