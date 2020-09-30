using System;
using Core.DomainModel;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Options
{
    public abstract class SingleOptionTypeInstanceAssignmentServiceBase<TOwner, TOption> : ISingleOptionTypeInstanceAssignmentService<TOwner, TOption>
        where TOption : OptionEntity<TOwner>
        where TOwner : IOwnedByOrganization
    {
        private readonly IOptionsService<TOwner, TOption> _optionsService;

        protected SingleOptionTypeInstanceAssignmentServiceBase(IOptionsService<TOwner, TOption> optionsService)
        {
            _optionsService = optionsService;
        }

        public Result<TOption, OperationError> Assign(TOwner owner, int optionId)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            return
                _optionsService
                    .GetAvailableOption(owner.OrganizationId, optionId)
                    .Select(option => PerformAssign(owner, option))
                    .Match(result => result, () => new OperationError("Invalid option id", OperationFailure.BadInput));

        }

        public Result<TOption, OperationError> Clear(TOwner owner)
        {
            return
                GetAssignmentState(owner)
                    .Match(
                        option => PerformClear(owner).Match<Result<TOption, OperationError>>(error => error, () => option),
                        () => new OperationError("Not assigned", OperationFailure.BadState)
                    );
        }

        protected abstract Result<TOption, OperationError> PerformAssign(TOwner owner, TOption option);
        protected abstract Maybe<OperationError> PerformClear(TOwner owner);
        protected abstract Maybe<TOption> GetAssignmentState(TOwner owner);
    }
}