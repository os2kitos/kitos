using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingAgreementSystemAssignmentService
    {
        IQueryable<ItSystem> GetApplicableSystems(DataProcessingAgreement agreement);
        Result<ItSystem, OperationError> AssignSystem(DataProcessingAgreement agreement, int systemId);
        Result<ItSystem, OperationError> RemoveSystem(DataProcessingAgreement agreement, int systemId);
    }
}
