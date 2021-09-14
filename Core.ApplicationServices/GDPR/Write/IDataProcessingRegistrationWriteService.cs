using System;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR.Write
{
    public interface IDataProcessingRegistrationWriteService
    {
        Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters);
        Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters);
        Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid);
    }
}
