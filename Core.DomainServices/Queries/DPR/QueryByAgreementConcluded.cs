using Core.DomainModel.GDPR;
using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryByAgreementConcluded : IDomainQuery<DataProcessingRegistration>
    {
        private readonly YesNoIrrelevantOption _isAgreementConcluded;

        public QueryByAgreementConcluded(YesNoIrrelevantOption isAgreementConcluded)
        {
            _isAgreementConcluded = isAgreementConcluded;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => dataProcessingRegistration.IsAgreementConcluded == _isAgreementConcluded);
        }
    }
}
