using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Queries.SystemUsage
{
    public class QueryReadModelsWhichShouldBecomeActive : IDomainQuery<ItSystemUsageOverviewReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldBecomeActive(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<ItSystemUsageOverviewReadModel> Apply(IQueryable<ItSystemUsageOverviewReadModel> source)
        {
            var currentTime = _currentTime.Date;
            return source.Where(
                x =>

                    // All currently inactive models
                    x.ActiveAccordingToValidityPeriod == false &&
                    (
                        (
                            // 1: Common scenario
                            // Include systems where concluded (start time) has passed or is not defined
                            (x.SourceEntity.Concluded == null || x.SourceEntity.Concluded <= currentTime) &&
                            // Include only if not expired or no expiration defined
                            (x.SourceEntity.ExpirationDate == null || currentTime <= x.SourceEntity.ExpirationDate)
                        )
                    ) ||
                    x.MainContractIsActive == MainContractState.Inactive &&
                    // Include if Main Contract is active
                    (
                        //Main Contract is null which means it's valid
                        x.SourceEntity.MainContract == null ||
                        (
                            // 1: Common scenario
                            // Exclude those which were enforced as valid - dates have no effect
                            x.SourceEntity.MainContract.ItContract.Active == false &&
                            // Include systems where concluded (start time) has passed or is not defined
                            (x.SourceEntity.MainContract.ItContract.Concluded == null || x.SourceEntity.MainContract.ItContract.Concluded <= currentTime) &&
                            // Include only if not expired or no expiration defined
                            (x.SourceEntity.MainContract.ItContract.ExpirationDate == null || currentTime <= x.SourceEntity.MainContract.ItContract.ExpirationDate) 
                        ) ||
                        // 2: Out of sync scenario
                        // Source entity marked as active (forced) but read model state false, mark as target for update
                        x.SourceEntity.MainContract.ItContract.Active == true
                    )


            );
        }
    }
}
