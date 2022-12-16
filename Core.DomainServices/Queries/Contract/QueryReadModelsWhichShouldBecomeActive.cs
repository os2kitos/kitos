using System;
using System.Linq;
using Core.DomainModel.ItContract.Read;
using Core.DomainServices.Queries.Helpers;

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
                    x.IsActive == false && ItContractIsActiveQueryHelper.CheckIfContractIsValid(currentTime, x.SourceEntity)
            );
        }
    }
}
