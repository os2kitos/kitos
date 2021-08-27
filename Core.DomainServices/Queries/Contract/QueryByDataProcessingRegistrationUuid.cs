using System;
using System.Linq;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Queries.Contract
{
    public class QueryByDataProcessingRegistrationUuid : IDomainQuery<ItContract>
    {
        private readonly Guid _dataProcessingRegistrationId;

        public QueryByDataProcessingRegistrationUuid(Guid dataProcessingRegistrationId)
        {
            _dataProcessingRegistrationId = dataProcessingRegistrationId;
        }

        public IQueryable<ItContract> Apply(IQueryable<ItContract> source)
        {
            return source.Where(contract => contract.DataProcessingRegistrations.Any(registration => registration.Uuid == _dataProcessingRegistrationId));
        }
    }
}
