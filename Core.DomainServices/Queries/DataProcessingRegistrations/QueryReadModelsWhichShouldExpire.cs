using System;
using System.Linq;
using Core.DomainModel.GDPR.Read;

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
                    // Remove results where the date has no effect (active overrides all other logic)
                    x.SourceEntity.MainContract.Active == false &&
                    (
                        // Expiration data defined
                        x.SourceEntity.MainContract.ExpirationDate != null &&
                        // Expiration date has passed
                        x.SourceEntity.MainContract.ExpirationDate < currentTime ||
                        // Termination data defined
                        x.SourceEntity.MainContract.Terminated != null &&
                        // Termination date defined
                        x.SourceEntity.MainContract.Terminated < currentTime
                    )
                )
            );
        }
    }
}
