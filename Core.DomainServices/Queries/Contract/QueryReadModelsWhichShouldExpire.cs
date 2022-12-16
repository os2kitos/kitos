using System;
using System.Linq;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Queries.Helpers;

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
                    x.IsActive && ItContractIsActiveQueryHelper.CheckIfContractIsExpired(currentTime, x.SourceEntity)
            );
        }
    }
}
