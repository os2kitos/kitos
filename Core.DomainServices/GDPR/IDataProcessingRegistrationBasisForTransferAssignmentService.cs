using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Options;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationBasisForTransferAssignmentService : IOptionTypeAssignmentService<DataProcessingRegistration,DataProcessingBasisForTransferOption>
    {
    }
}
