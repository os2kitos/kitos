using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.Queries.Helpers;

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
                    (
                        x.ActiveAccordingToValidityPeriod &&
                        // Expiration data defined
                        x.SourceEntity.ExpirationDate != null &&
                        // Expiration date has passed
                        x.SourceEntity.ExpirationDate < currentTime) ||
                        // All currently set as active in the read model
                        (x.MainContractIsActive &&
                        // Main Contract is not null
                        x.SourceEntity.MainContract != null &&
                        ItContractIsActiveQueryHelper.CheckIfContractIsExpired(currentTime, x.SourceEntity.MainContract.ItContract)
                    )
            );
        }
    }
}
