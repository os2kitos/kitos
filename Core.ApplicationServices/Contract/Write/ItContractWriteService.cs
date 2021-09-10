using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
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
        private readonly IGenericRepository<ItContractAgreementElementTypes> _itContractAgreementElementTypesRepository;
        private readonly IAuthorizationContext _authorizationContext;

        public ItContractWriteService(
            IItContractService contractService,
            IEntityIdentityResolver entityIdentityResolver,
            IOptionResolver optionResolver,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            IGenericRepository<ItContractAgreementElementTypes> itContractAgreementElementTypesRepository,
            IAuthorizationContext authorizationContext)
        {
            _contractService = contractService;
            _entityIdentityResolver = entityIdentityResolver;
            _optionResolver = optionResolver;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _itContractAgreementElementTypesRepository = itContractAgreementElementTypesRepository;
            _authorizationContext = authorizationContext;
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
                .Bind(WithWriteAccess)
                .Bind(contract => ApplyUpdates(contract, parameters));

            if (updateResult.Ok)
            {
                _domainEvents.Raise(new EntityUpdatedEvent<ItContract>(updateResult.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return updateResult;
        }

        private Result<ItContract, OperationError> WithWriteAccess(ItContract contract)
        {
            if (!_authorizationContext.AllowModify(contract))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return contract;
        }

        private Result<ItContract, OperationError> ApplyUpdates(ItContract contract, ItContractModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.Name, UpdateName)
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.ParentContractUuid, UpdateParentContract))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.General, UpdateGeneralData))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Procurement, UpdateProcurement))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.SystemUsageUuids, UpdateSystemAssignments));
        }

        private Result<ItContract, OperationError> UpdateSystemAssignments(ItContract contract, IEnumerable<Guid> systemUsageUuids)
        {
            var usageUuids = systemUsageUuids.ToList();
            if (usageUuids.Distinct().Count() != usageUuids.Count)
            {
                return new OperationError($"Duplicates of 'SystemUsages' are not allowed", OperationFailure.BadInput);
            }

            var existingUuids = contract.AssociatedSystemUsages.Select(x => x.ItSystemUsage.Uuid).ToList();

            var changes = existingUuids.ComputeDelta(usageUuids, uuid => uuid).ToList();

            foreach (var (delta, uuid) in changes)
            {
                switch (delta)
                {
                    case EnumerableExtensions.EnumerableDelta.Added:
                        var usageId = _entityIdentityResolver.ResolveDbId<ItSystemUsage>(uuid);

                        if (usageId.IsNone)
                            return new OperationError($"No SystemUsage found with Uuid: {uuid}", OperationFailure.BadInput);


                        contract.AssociatedSystemUsages.Add(new ItContractItSystemUsage()
                        {
                            ItContractId = contract.Id,
                            ItSystemUsageId = usageId.Value
                        });

                        break;
                    case EnumerableExtensions.EnumerableDelta.Removed:
                        var associationToRemove = contract.AssociatedSystemUsages.Single(x => x.ItSystemUsage.Uuid == uuid);
                        var removeSucceeded = contract.AssociatedSystemUsages.Remove(associationToRemove);

                        if (!removeSucceeded)
                            return new OperationError(
                                $"Failed to remove Associated SystemUsage with Uuid: {uuid} from Contract with Uuid: {contract.Uuid}",
                                OperationFailure.BadState);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return contract;
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

        private Maybe<OperationError> UpdateAgreementElements(ItContract contract, IEnumerable<Guid> agreementElements)
        {
            var agreementElementTypes = new List<AgreementElementType>();
            foreach (var uuid in agreementElements)
            {
                var result = _optionResolver.GetOptionType<ItContract, AgreementElementType>(contract.Organization.Uuid, uuid);
                if (result.Failed)
                {
                    return new OperationError($"Failed resolving agreement element with uuid:{uuid}. Message:{result.Error.Message.GetValueOrEmptyString()}", result.Error.FailureType);
                }

                var resultValue = result.Value;
                if (resultValue.available == false && contract.AssociatedAgreementElementTypes.Any(x => x.AgreementElementType.Uuid == uuid) == false)
                {
                    return new OperationError($"Tried to add agreement element which is not available in the organization: {uuid}", OperationFailure.BadInput);
                }
                agreementElementTypes.Add(resultValue.option);
            }

            var before = contract.AssociatedAgreementElementTypes.ToList();

            var error = contract.SetAgreementElements(agreementElementTypes);
            if (error.IsNone)
            {
                var after = before.Except(contract.AssociatedAgreementElementTypes);
                after.ToList().ForEach(_itContractAgreementElementTypesRepository.Delete);
            }
            return error;
        }

        private static Maybe<OperationError> UpdateValidityPeriod(ItContract itContract, ItContractGeneralDataModificationParameters generalData)
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

        private Result<ItContract, OperationError> UpdateProcurement(ItContract contract, ItContractProcurementModificationParameters procurementParameters)
        {
            return contract
                .WithOptionalUpdate(procurementParameters.ProcurementStrategyUuid, UpdateProcurementStrategy)
                .Bind(itContract => itContract.WithOptionalUpdate(procurementParameters.PurchaseTypeUuid, UpdatePurchaseType))
                .Bind(itContract => itContract.WithOptionalUpdate(procurementParameters.ProcurementPlan, UpdateProcurementPlan));
        }

        private static Maybe<OperationError> UpdateProcurementPlan(ItContract contract, Maybe<(byte half, int year)> plan)
        {
            if (plan.IsNone)
            {
                contract.ResetProcurementPlan();
                return Maybe<OperationError>.None;
            }

            var updateResult = contract.UpdateProcurementPlan(plan.Value);
            return updateResult.HasValue 
                ? new OperationError($"Failed to update procurement plan with error message: {updateResult.Value.Message.GetValueOrEmptyString()}", updateResult.Value.FailureType) 
                : Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdatePurchaseType(ItContract contract, Guid? purchaseTypeUuid)
        {
            return UpdateIndependentOptionTypeAssignment(
                contract,
                purchaseTypeUuid,
                c => c.ResetPurchaseForm(),
                c => c.PurchaseForm,
                (c, newValue) => c.PurchaseForm = newValue
            );
        }

        private Maybe<OperationError> UpdateProcurementStrategy(ItContract contract, Guid? procurementStrategyUuid)
        {
            return UpdateIndependentOptionTypeAssignment(
                contract,
                procurementStrategyUuid,
                c => c.ResetProcurementStrategy(),
                c => c.ProcurementStrategy,
                (c, newValue) => c.ProcurementStrategy = newValue
            );
        }

        private Maybe<OperationError> UpdateParentContract(ItContract contract, Guid? newParentUuid)
        {
            if (!newParentUuid.HasValue)
            {
                contract.ClearParent();
                return Maybe<OperationError>.None;
            }

            var getResult = _contractService.GetContract(newParentUuid.Value);

            if (getResult.Failed)
                return new OperationError($"Failed to get contract with Uuid: {newParentUuid.Value} with error message: {getResult.Error.Message.GetValueOrEmptyString()}", getResult.Error.FailureType);

            return contract
                .SetParent(getResult.Value)
                .Match
                (
                    error => new OperationError($"Failed to set parent with Uuid: {newParentUuid.Value} on contract with Uuid: {contract.Uuid} with error message: {error.Message.GetValueOrEmptyString()}",
                        error.FailureType), () => Maybe<OperationError>.None
                );
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
                    return new OperationError($"Failure while resolving {typeof(TOption).Name} option:{optionType.Error.Message.GetValueOrEmptyString()}", optionType.Error.FailureType);
                }

                var option = optionType.Value;
                var currentValue = getCurrentValue(contract);
                if (option.available == false && (currentValue == null || currentValue.Uuid != optionTypeUuid.Value))
                {
                    return new OperationError($"The changed {typeof(TOption).Name} points to an option which is not available in the organization", OperationFailure.BadInput);
                }

                updateValue(contract, option.option);
            }

            return Maybe<OperationError>.None;
        }
    }
}
