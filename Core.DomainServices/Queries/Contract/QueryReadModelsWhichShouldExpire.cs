using System;
using System.Linq;
using Core.DomainModel.ItContract.Read;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryReadModelsWhichShouldExpire : IDomainQuery<ItContractOverviewReadModel>
    {
        private readonly DateTime _currentTime;

        public QueryReadModelsWhichShouldExpire(DateTime currentTime)
        {
            _currentTime = currentTime;
        }

        public IQueryable<ItContractOverviewReadModel> Apply(IQueryable<ItContractOverviewReadModel> source)
        {
            var currentTime = _currentTime.Date;

            return source.Where(

                x =>
                    // All currently set as active in the read model
                    x.IsActive &&
                    x.SourceEntity.Active == false &&
                    (
                        // Expiration data defined
                        x.SourceEntity.ExpirationDate != null &&
                        // Expiration date has passed
                        x.SourceEntity.ExpirationDate < currentTime ||
                        // Termination data defined
                        x.SourceEntity.Terminated != null &&
                        // Termination date defined
                        x.SourceEntity.Terminated < currentTime
                    )
            );
                    //ItContractIsActiveQueryHelper.CheckIfContractIsExpired(currentTime, x.SourceEntity)
        }
    }
}
