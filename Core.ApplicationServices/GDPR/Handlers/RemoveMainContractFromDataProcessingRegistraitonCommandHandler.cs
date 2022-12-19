using System;
using Core.Abstractions.Types;
using Core.DomainModel.Commands;

namespace Core.ApplicationServices.GDPR.Handlers
{
    public class RemoveMainContractFromDataProcessingRegistraitonCommandHandler : ICommandHandler<RemoveMainContractFromDataProcessingRegistrationCommand, Maybe<OperationError>>
    {
        public Maybe<OperationError> Execute(RemoveMainContractFromDataProcessingRegistrationCommand command)
        {
            var removedDpr = command.RemovedDataProcessingRegistration;
            removedDpr.ResetMainContract();

            return Maybe<OperationError>.None;
        }
    }
}
