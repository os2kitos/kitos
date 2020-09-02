using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingAgreementDomainService
    {
        Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name);
        Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name);
        Maybe<OperationError> ChangeName(DataProcessingAgreement dataProcessingAgreement, string newName);
    }
}
