using System;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationOversightDateAssignmentService : IDataProcessingRegistrationOversightDateAssignmentService
    {
        private readonly IGenericRepository<DataProcessingRegistrationOversightDate> _repository;

        public DataProcessingRegistrationOversightDateAssignmentService(IGenericRepository<DataProcessingRegistrationOversightDate> repository)
        {
            _repository = repository;
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> Assign(DataProcessingRegistration registration, DateTime? oversightDate, string oversightRemark)
        {
            return registration.AssignOversightDate(oversightDate, oversightRemark);
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> Modify(DataProcessingRegistration registration, int oversightId, DateTime? oversightDate, string oversightRemark)
        {
            return registration.ModifyOversightDate(oversightId, oversightDate, oversightRemark);
        }

        public Result<DataProcessingRegistrationOversightDate, OperationError> Remove(DataProcessingRegistration registration, int oversightId)
        {
            var removedRegistration = registration.RemoveOversightDate(oversightId);
            if (removedRegistration.Ok)
            {
                _repository.Delete(removedRegistration.Value);
            }
            return removedRegistration;
        }
    }
}
