using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Options;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationBasisForTransferAssignmentService : OptionTypeAssignmentServiceBase<DataProcessingRegistration,DataProcessingBasisForTransferOption>,IDataProcessingRegistrationBasisForTransferAssignmentService
    {
        public DataProcessingRegistrationBasisForTransferAssignmentService(IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> optionsService) : base(optionsService)
        {
        }

        protected override Result<DataProcessingBasisForTransferOption, OperationError> PerformAssign(DataProcessingRegistration owner, DataProcessingBasisForTransferOption option)
        {
            owner.BasisForTransfer = option;
            return option;
        }

        protected override Result<DataProcessingBasisForTransferOption, OperationError> PerformRemove(DataProcessingRegistration owner, DataProcessingBasisForTransferOption option)
        {
            if(option.Id != owner.BasisForTransferId)
                return new OperationError("Option id does not point to the assigned option", OperationFailure.BadInput);
            
            owner.BasisForTransfer = null;
            return option;
        }
    }
}
