using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Options;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationDataResponsibleAssigmentService : 
        SingleOptionTypeInstanceAssignmentServiceBase<DataProcessingRegistration, DataProcessingDataResponsibleOption>,
        IDataProcessingRegistrationDataResponsibleAssignmentService
    {
        public DataProcessingRegistrationDataResponsibleAssigmentService(
            IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption> optionsService)
            : base(optionsService)
        { }

        protected override Result<DataProcessingDataResponsibleOption, OperationError> PerformAssign(DataProcessingRegistration owner, DataProcessingDataResponsibleOption option)
        {
            owner.DataResponsible = option;
            return option;
        }

        protected override Maybe<OperationError> PerformClear(DataProcessingRegistration owner)
        {
            owner.DataResponsible = null;
            return Maybe<OperationError>.None;
        }

        protected override Maybe<DataProcessingDataResponsibleOption> GetAssignmentState(DataProcessingRegistration owner)
        {
            return owner.DataResponsible;
        }

    }
}
