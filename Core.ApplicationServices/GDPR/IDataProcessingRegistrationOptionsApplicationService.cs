using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GDPR;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationOptionsApplicationService
    {
        Result<DataProcessingRegistrationOptions, OperationError> GetAssignableDataProcessingRegistrationOptions(int organizationId);
    }
}
