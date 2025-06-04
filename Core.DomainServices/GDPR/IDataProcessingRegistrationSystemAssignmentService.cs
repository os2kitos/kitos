using System.Linq;
using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationSystemAssignmentService
    {
        IQueryable<ItSystemUsage> GetApplicableSystems(DataProcessingRegistration registration);
        Result<ItSystemUsage, OperationError> AssignSystem(DataProcessingRegistration registration, int systemId);
        Result<ItSystemUsage, OperationError> RemoveSystem(DataProcessingRegistration registration, int systemId);
    }
}
