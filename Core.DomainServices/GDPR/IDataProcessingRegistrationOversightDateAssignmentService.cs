using System;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationOversightDateAssignmentService
    {
        public Result<DataProcessingRegistrationOversightDate, OperationError> Assign(DataProcessingRegistration registration, DateTime oversightDate, string oversightRemark);
        public Result<DataProcessingRegistrationOversightDate, OperationError> Remove(DataProcessingRegistration registration, int oversightId);
        public Result<DataProcessingRegistrationOversightDate, OperationError> Modify(DataProcessingRegistration registration, int oversightId, DateTime oversightDate, string oversightRemark);
    }
}
