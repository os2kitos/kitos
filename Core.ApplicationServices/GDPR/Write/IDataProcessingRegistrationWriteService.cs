using Core.Abstractions.Types;
using Core.ApplicationServices.Model.GDPR.Write;
using Core.ApplicationServices.Model.Shared.Write;
using Core.DomainModel.GDPR;
using System;


namespace Core.ApplicationServices.GDPR.Write
{
    public interface IDataProcessingRegistrationWriteService
    {
        Result<DataProcessingRegistration, OperationError> Create(Guid organizationUuid, DataProcessingRegistrationCreationParameters parameters);
        Result<DataProcessingRegistration, OperationError> Update(Guid dataProcessingRegistrationUuid, DataProcessingRegistrationModificationParameters parameters);
        Maybe<OperationError> Delete(Guid dataProcessingRegistrationUuid);
        Result<DataProcessingRegistration, OperationError> AddRole(Guid dprUuid, UserRolePair assignment);
        Result<DataProcessingRegistration, OperationError> RemoveRole(Guid dprUuid, UserRolePair assignment);

        Result<DataProcessingRegistrationOversightDate, OperationError> AddOversightDate(
            Guid dataProcessingRegistrationUuid, UpdatedDataProcessingRegistrationOversightDateParameters parameters);

        Result<DataProcessingRegistrationOversightDate, OperationError> UpdateOversightDate(
            Guid dataProcessingRegistrationUuid, Guid oversightDateUuid,
            UpdatedDataProcessingRegistrationOversightDateParameters parameters);

        Maybe<OperationError> DeleteOversightDate(Guid dataProcessingRegistrationUuid, Guid oversightDateUuid);
    }
}
