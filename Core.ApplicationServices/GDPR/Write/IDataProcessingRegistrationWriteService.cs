using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.GDPR;


namespace Core.ApplicationServices.GDPR.Write
{
    public interface IDataProcessingRegistrationWriteService
    {
        Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationModificationParameters parameters);
        Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters);
        Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid);
        Result<DataProcessingRegistration, OperationError> AddRole(Guid dprUuid, UserRolePair assignment);
        Result<DataProcessingRegistration, OperationError> RemoveRole(Guid dprUuid, UserRolePair assignment);
    }
}
