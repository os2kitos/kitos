using System;
using System.Collections.Generic;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;


namespace Core.ApplicationServices.Contract.Write
{
    public class ItContractWriteService : IItContractWriteService
    {
        private readonly IItContractService _contractService;
        private readonly IEntityIdentityResolver _entityIdentityResolver;
        private readonly IOptionResolver _optionResolver;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IDatabaseControl _databaseControl;

        public ItContractWriteService(
            IItContractService contractService,
            IEntityIdentityResolver entityIdentityResolver,
            IOptionResolver optionResolver,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl)
        {
            _contractService = contractService;
            _entityIdentityResolver = entityIdentityResolver;
            _optionResolver = optionResolver;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
        }

        public Result<ItContract, OperationError> Create(Guid organizationUuid, ItContractModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (parameters.Name.IsUnchanged)
                return new OperationError("Name must be provided", OperationFailure.BadInput);

            using var transaction = _transactionManager.Begin();

            var orgId = _entityIdentityResolver.ResolveDbId<Organization>(organizationUuid);
            if (orgId.IsNone)
                return new OperationError("Organization id not valid", OperationFailure.BadInput);

            var nameNewValue = parameters.Name.NewValue;

            parameters.Name = OptionalValueChange<string>.None; //Do not update after creation

            var result = _contractService
                .Create(orgId.Value, nameNewValue)
                .Bind(contract => ApplyUpdates(contract, parameters));

            if (result.Ok)
            {
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return result;
        }

        public Result<ItContract, OperationError> Update(Guid itContractUuid, ItContractModificationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using var transaction = _transactionManager.Begin();

            var updateResult = _contractService
                .GetContract(itContractUuid)
                .Bind(contract => ApplyUpdates(contract, parameters));

            if (updateResult.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(updateResult.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return updateResult;
        }

        private Result<ItContract, OperationError> ApplyUpdates(ItContract contract, ItContractModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.Name, UpdateName)
                .Bind(contract => contract.WithOptionalUpdate(parameters.General, UpdateGeneralData));
        }

        private Result<ItContract, OperationError> UpdateGeneralData(ItContract contract, ItContractGeneralDataModificationParameters generalData)
        {
            return contract
                .WithOptionalUpdate(generalData.ContractId, (c, newValue) => c.ItContractId = newValue)
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.ContractTypeUuid, UpdateContractType))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.ContractTemplateUuid, UpdateContractTemplate))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.Notes, (c, newValue) => c.Note = newValue))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.EnforceValid, (c, newValue) => c.Active = newValue.GetValueOrFallback(false)))
                .Bind(itContract => UpdateValidityPeriod(itContract, generalData).Match<Result<ItContract, OperationError>>(error => error, () => itContract))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.AgreementElementUuids, UpdateAgreementElements));
        }

        private Maybe<OperationError> UpdateAgreementElements(ItContract arg1, IEnumerable<Guid> arg2)
        {
            throw new NotImplementedException();
        }

        private Maybe<OperationError> UpdateValidityPeriod(ItContract itContract, ItContractGeneralDataModificationParameters generalData)
        {
            if (generalData.ValidFrom.IsUnchanged && generalData.ValidTo.IsUnchanged)
                return Maybe<OperationError>.None;

            var newValidFrom = generalData.ValidFrom.MapDateTimeOptionalChangeWithFallback(itContract.Concluded);
            var newValidTo = generalData.ValidTo.MapDateTimeOptionalChangeWithFallback(itContract.ExpirationDate);

            return itContract.UpdateContractValidityPeriod(newValidFrom, newValidTo);
        }

        private Maybe<OperationError> UpdateContractTemplate(ItContract contract, Guid? contractTemplateUuid)
        {
            return UpdateIndependentOptionTypeAssignment
            (
                contract,
                contractTemplateUuid,
                c => c.ResetContractTemplate(),
                c => c.ContractTemplate,
                (c, newValue) => c.ContractTemplate = newValue
            );
        }

        private Maybe<OperationError> UpdateContractType(ItContract contract, Guid? contractTypeId)
        {
            return UpdateIndependentOptionTypeAssignment
            (
                contract,
                contractTypeId,
                c => c.ResetContractType(),
                c => c.ContractType,
                (c, newValue) => c.ContractType = newValue
            );
        }

        private Maybe<OperationError> UpdateIndependentOptionTypeAssignment<TOption>(
            ItContract contract,
            Guid? optionTypeUuid,
            Action<ItContract> onReset,
            Func<ItContract, TOption> getCurrentValue,
            Action<ItContract, TOption> updateValue) where TOption : OptionEntity<ItContract>
        {
            if (optionTypeUuid == null)
            {
                onReset(contract);
            }
            else
            {
                var optionType = _optionResolver.GetOptionType<ItContract, TOption>(contract.Organization.Uuid, optionTypeUuid.Value);
                if (optionType.Failed)
                {
                    return new OperationError($"Failure while resolving {typeof(TOption).Namespace} option:{optionType.Error.Message.GetValueOrEmptyString()}", optionType.Error.FailureType);
                }

                var option = optionType.Value;
                var currentValue = getCurrentValue(contract);
                if (option.available == false && (currentValue == null || currentValue.Uuid != optionTypeUuid.Value))
                {
                    return new OperationError($"The changed {typeof(TOption).Namespace} points to an option which is not available in the organization", OperationFailure.BadInput);
                }

                updateValue(contract, option.option);
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdateName(ItContract contract, string newName)
        {
            var error = _contractService.ValidateNewName(contract.Id, newName);

            if (error.HasValue)
                return error;

            contract.Name = newName;
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> Delete(Guid itContractUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<ItContract>(itContractUuid);

            if (dbId.IsNone)
                return new OperationError("Invalid contract uuid", OperationFailure.NotFound);

            return _contractService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, failure => new OperationError("Failed deleting contract", failure));
        }
    }
}
