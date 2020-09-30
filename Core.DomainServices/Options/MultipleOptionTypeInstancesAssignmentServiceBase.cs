using System;
using Core.DomainModel;
using Core.DomainModel.Result;

namespace Core.DomainServices.Options
{
    public abstract class MultipleOptionTypeInstancesAssignmentServiceBase<TOwner, TOption> : IMultipleOptionTypeInstancesAssignmentService<TOwner, TOption>
        where TOption : OptionEntity<TOwner>
        where TOwner : IOwnedByOrganization
    {
        private readonly IOptionsService<TOwner, TOption> _optionsService;

        protected MultipleOptionTypeInstancesAssignmentServiceBase(IOptionsService<TOwner, TOption> optionsService)
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

        public Result<TOption, OperationError> Remove(TOwner owner, int optionId)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            return
                _optionsService
                    .GetOption(owner.OrganizationId, optionId) //Option may have been deprecated so allow it not to be available
                    .Select(x => x.option)
                    .Select(option => PerformRemove(owner, option))
                    .Match(result => result, () => new OperationError("Invalid option id", OperationFailure.BadInput));
        }

        protected abstract Result<TOption, OperationError> PerformAssign(TOwner owner, TOption option);
        protected abstract Result<TOption, OperationError> PerformRemove(TOwner owner, TOption option);
    }
}
