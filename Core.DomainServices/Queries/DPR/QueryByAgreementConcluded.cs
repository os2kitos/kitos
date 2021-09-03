using Core.DomainModel.GDPR;
using System.Linq;
using Core.DomainModel.Shared;

namespace Core.DomainServices.Queries.DPR
{
    public class QueryByAgreementConcluded : IDomainQuery<DataProcessingRegistration>
    {
        private readonly bool _isAgreementConcluded;

        public QueryByAgreementConcluded(bool isAgreementConcluded)
        {
            _isAgreementConcluded = isAgreementConcluded;
        }

        public IQueryable<DataProcessingRegistration> Apply(IQueryable<DataProcessingRegistration> source)
        {
            return source.Where(dataProcessingRegistration => 
                _isAgreementConcluded 
                    ? dataProcessingRegistration.IsAgreementConcluded == YesNoIrrelevantOption.YES
                    : (dataProcessingRegistration.IsAgreementConcluded == null || dataProcessingRegistration.IsAgreementConcluded != YesNoIrrelevantOption.YES));
        }
    }
}
