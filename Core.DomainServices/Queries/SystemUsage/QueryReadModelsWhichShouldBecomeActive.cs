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
                    )
            );
        }
    }
}
