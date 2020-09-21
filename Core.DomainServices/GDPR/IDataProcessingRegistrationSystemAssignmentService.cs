using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationSystemAssignmentService
    {
        IQueryable<ItSystem> GetApplicableSystems(DataProcessingRegistration registration);
        Result<ItSystem, OperationError> AssignSystem(DataProcessingRegistration registration, int systemId);
        Result<ItSystem, OperationError> RemoveSystem(DataProcessingRegistration registration, int systemId);
    }
}
