using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Options;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationOversightOptionsAssignmentService :
        MultipleOptionTypeInstancesAssignmentServiceBase<DataProcessingRegistration, DataProcessingOversightOption>,
        IDataProcessingRegistrationOversightOptionsAssignmentService
    {
        public DataProcessingRegistrationOversightOptionsAssignmentService(IOptionsService<DataProcessingRegistration, DataProcessingOversightOption> optionsService)
        : base(optionsService) { }

        protected override Result<DataProcessingOversightOption, OperationError> PerformAssign(DataProcessingRegistration owner, DataProcessingOversightOption option)
        {
            return owner.AssignOversightOption(option);
        }

        protected override Result<DataProcessingOversightOption, OperationError> PerformRemove(DataProcessingRegistration owner, DataProcessingOversightOption option)
        {
            return owner.RemoveOversightOption(option);
        }
    }
}
