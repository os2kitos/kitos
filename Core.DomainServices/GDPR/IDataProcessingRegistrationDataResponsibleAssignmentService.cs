using System.Collections.Generic;
using Core.DomainModel.GDPR;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationDataResponsibleAssignmentService
    {
        IEnumerable<DataProcessingDataResponsibleOption> GetApplicableDataResponsibleOptions(DataProcessingRegistration registration);
    }
}
