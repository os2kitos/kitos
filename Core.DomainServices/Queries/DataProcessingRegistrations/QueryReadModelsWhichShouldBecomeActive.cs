using System;
using System.Linq;
using Core.DomainModel.GDPR.Read;

namespace Core.DomainServices.Queries.DataProcessingRegistrations
{
    public class QueryReadModelsWhichShouldBecomeActive : IDomainQuery<DataProcessingRegistrationReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldBecomeActive(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<DataProcessingRegistrationReadModel> Apply(IQueryable<DataProcessingRegistrationReadModel> source)
        {
            var currentTime = _currentTime.Date;
            return source.Where(
                x =>
                (
                    x.ActiveAccordingToMainContract == false &&
                    (
                        x.SourceEntity.MainContract == null ||
                        (
                            // 1: Common scenario
                            // Exclude those which were enforced as valid - dates have no effect
                            x.SourceEntity.MainContract.Active == false &&
                            // Include where concluded (start time) has passed or is not defined
                            (x.SourceEntity.MainContract.Concluded == null || x.SourceEntity.MainContract.Concluded <= currentTime) &&
                            // Include only if not expired or no expiration defined
                            (x.SourceEntity.MainContract.ExpirationDate == null || currentTime <= x.SourceEntity.MainContract.ExpirationDate)
                        ) ||
                        // 2: Out of sync scenario
                        // Source entity marked as active (forced) but read model state false, mark as target for update
                        x.SourceEntity.MainContract.Active
                    )
                )
            );
        }
    }
}
