using Core.DomainModel.GDPR;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingAgreementService
    {
        Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name);
        Result<DataProcessingAgreement, OperationError> Delete(int id);
        Result<DataProcessingAgreement, OperationError> Get(int id);
    }
}
