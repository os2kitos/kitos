using System;
using System.Linq;
using Core.DomainModel.ItSystemUsage.Read;

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

                    // 1: Common scenario
                    (
                        // All currently inactive models
                        x.IsActive == false &&
                        // Exclude those which were enforced as valid - dates have no effect
                        x.SourceEntity.Active == false &&
                        // Include systems where concluded (start time) has passed or is not defined
                        (x.SourceEntity.Concluded == null || x.SourceEntity.Concluded >= currentTime) &&
                        // Include only if not expired or no expiration defined
                        (x.SourceEntity.ExpirationDate == null || currentTime <= x.SourceEntity.ExpirationDate)
                        ) ||

                    // 2: Out of sync scenario
                    // Source entity marked as active (forced) but read model state false, mark as target for update
                    x.SourceEntity.Active == true

            );
        }
    }
}
