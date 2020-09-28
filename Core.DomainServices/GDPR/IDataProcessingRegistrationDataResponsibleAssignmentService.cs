using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationDataResponsibleAssignmentService
    {
        IEnumerable<DataProcessingDataResponsibleOption> GetApplicableDataResponsibleOptionsWithLocalDescriptionOverrides(DataProcessingRegistration registration);
        Result<DataProcessingRegistration, OperationError> UpdateDataResponsible(DataProcessingRegistration registration, int? dataResponsibleOptionId);
    }
}
