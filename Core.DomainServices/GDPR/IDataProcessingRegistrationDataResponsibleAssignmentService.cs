using Core.DomainModel.GDPR;
using Core.DomainServices.Options;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationDataResponsibleAssignmentService : ISingleOptionTypeInstanceAssignmentService<DataProcessingRegistration, DataProcessingDataResponsibleOption>
    {
    }
}
