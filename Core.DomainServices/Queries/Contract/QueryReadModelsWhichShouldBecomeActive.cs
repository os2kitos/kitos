using System;
using System.Linq;
using Core.DomainModel.ItContract.Read;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryReadModelsWhichShouldBecomeActive : IDomainQuery<ItContractOverviewReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldBecomeActive(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<ItContractOverviewReadModel> Apply(IQueryable<ItContractOverviewReadModel> source)
        {
            var currentTime = _currentTime.Date;
            return source.Where(
                x =>
                    // All currently inactive models
                    x.IsActive == false &&
                    (
                        (
                            // 1: Common scenario
                            // Exclude those which were enforced as valid - dates have no effect
                            x.SourceEntity.Active == false &&
                            // Include systems where concluded (start time) has passed or is not defined
                            (x.SourceEntity.Concluded == null || x.SourceEntity.Concluded <= currentTime) &&
                            // Include only if not expired or no expiration defined
                            (x.SourceEntity.ExpirationDate == null || currentTime <= x.SourceEntity.ExpirationDate)
                        ) ||
                        // 2: Out of sync scenario
                        // Source entity marked as active (forced) but read model state false, mark as target for update
                        x.SourceEntity.Active == true
                    )
            );
        }
    }
}
