
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;

namespace Core.DomainServices.Contract
{
    public interface IContractDataProcessingRegistrationAssignmentService
    {
        Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(ItContract contract, int dataProcessingRegistrationId);
        Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(ItContract contract, int dataProcessingRegistrationId);
        IQueryable<DataProcessingRegistration> GetApplicableDataProcessingRegistrations(ItContract contract);
    }
}
