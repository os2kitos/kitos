﻿using Core.Abstractions.Types;
using Core.DomainModel.GDPR;
using Core.DomainServices.Options;


namespace Core.DomainServices.GDPR
{
    public class DataProcessingRegistrationBasisForTransferAssignmentService : SingleOptionTypeInstanceAssignmentServiceBase<DataProcessingRegistration, DataProcessingBasisForTransferOption>, IDataProcessingRegistrationBasisForTransferAssignmentService
    {
        public DataProcessingRegistrationBasisForTransferAssignmentService(IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption> optionsService) : base(optionsService)
        {
        }

        protected override Result<DataProcessingBasisForTransferOption, OperationError> PerformAssign(DataProcessingRegistration owner, DataProcessingBasisForTransferOption option)
        {
            owner.BasisForTransfer = option;
            return option;
        }

        protected override Maybe<OperationError> PerformClear(DataProcessingRegistration owner)
        {
            owner.BasisForTransfer = null;
            return Maybe<OperationError>.None;
        }


        protected override Maybe<DataProcessingBasisForTransferOption> GetAssignmentState(DataProcessingRegistration owner) => owner.BasisForTransfer;
    }
}
