using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Shared;

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
                    (x.ActiveAccordingToValidityPeriod &&
                     // Expiration data defined
                     x.SourceEntity.ExpirationDate != null &&
                     // Expiration date has passed
                     x.SourceEntity.ExpirationDate < currentTime) ||
                    // All currently set as active in the read model
                    (x.MainContractIsActive == MainContractState.Active &&
                     // Main Contract is not null
                     x.SourceEntity.MainContract != null &&
                     // Remove results where the date has no effect (active overrides all other logic)
                     x.SourceEntity.MainContract.ItContract.Active == false &&
                     (
                         // Expiration data defined
                         x.SourceEntity.MainContract.ItContract.ExpirationDate != null &&
                         // Expiration date has passed
                         x.SourceEntity.MainContract.ItContract.ExpirationDate < currentTime ||
                         // Termination data defined
                         x.SourceEntity.MainContract.ItContract.Terminated != null &&
                         // Termination date defined
                         x.SourceEntity.MainContract.ItContract.Terminated < currentTime
                     )
                    )
            );
        }
    }
}
