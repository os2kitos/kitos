using Core.Abstractions.Types;
using Core.DomainModel.GDPR;


namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationNamingService
    {
        Maybe<OperationError> ValidateSuggestedNewRegistrationName(int organizationId, string name);
        Maybe<OperationError> ChangeName(DataProcessingRegistration dataProcessingRegistration, string newName);
    }
}
