using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationNamingService
    {
        Maybe<OperationError> ValidateSuggestedNewRegistrationName(int organizationId, string name);
        Maybe<OperationError> ChangeName(DataProcessingRegistration dataProcessingRegistration, string newName);
    }
}
