using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainServices;
using Core.DomainServices.Generic;
using Core.DomainServices.Role;
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
        private readonly IOrganizationService _organizationService;
        private readonly IGenericRepository<HandoverTrial> _handoverTrialRepository;
        private readonly IReferenceService _referenceService;
        private readonly IAssignmentUpdateService _assignmentUpdateService;
        private readonly IItSystemUsageService _usageService;
        private readonly IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> _roleAssignmentService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IGenericRepository<PaymentMilestone> _paymentMilestoneRepository;

        public ItContractWriteService(
            IItContractService contractService,
            IEntityIdentityResolver entityIdentityResolver,
            IOptionResolver optionResolver,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            IGenericRepository<ItContractAgreementElementTypes> itContractAgreementElementTypesRepository,
            IAuthorizationContext authorizationContext,
            IOrganizationService organizationService,
            IGenericRepository<HandoverTrial> handoverTrialRepository,
            IReferenceService referenceService,
            IAssignmentUpdateService assignmentUpdateService,
            IItSystemUsageService usageService,
            IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> roleAssignmentService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IGenericRepository<PaymentMilestone> paymentMilestoneRepository)
        {
            _contractService = contractService;
            _entityIdentityResolver = entityIdentityResolver;
            _optionResolver = optionResolver;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _itContractAgreementElementTypesRepository = itContractAgreementElementTypesRepository;
            _authorizationContext = authorizationContext;
            _organizationService = organizationService;
            _handoverTrialRepository = handoverTrialRepository;
            _referenceService = referenceService;
            _assignmentUpdateService = assignmentUpdateService;
            _usageService = usageService;
            _roleAssignmentService = roleAssignmentService;
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _paymentMilestoneRepository = paymentMilestoneRepository;
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
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Responsible, UpdateResponsibleData))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Supplier, UpdateSupplierData))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.DataProcessingRegistrationUuids, UpdateDataProcessingRegistrations))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.SystemUsageUuids, UpdateSystemAssignments))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.HandoverTrials, UpdateHandOverTrials))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.PaymentModel, UpdatePaymentModelParameters))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.ExternalReferences, UpdateExternalReferences))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Roles, UpdateRoles))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Termination, UpdateTermination))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.AgreementPeriod, UpdateAgreementPeriod));
        }

        private Result<ItContract, OperationError> UpdateAgreementPeriod(ItContract contract, ItContractAgreementPeriodModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.IrrevocableUntil, (itContract, newValue) => itContract.IrrevocableTo = newValue)
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.ExtensionOptionsUuid, UpdateExtensionOption))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.ExtensionOptionsUsed, (itContract, newValue) => itContract.UpdateExtendMultiplier(newValue)))
                .Bind(updatedContract => UpdateDuration(updatedContract, parameters));
        }

        private static Result<ItContract, OperationError> UpdateDuration(ItContract contract, ItContractAgreementPeriodModificationParameters parameters)
        {
            if (parameters.DurationMonths.IsUnchanged && parameters.DurationYears.IsUnchanged && parameters.IsContinuous.IsUnchanged)
                return contract;

            var durationMonths = parameters.DurationMonths.MapOptionalChangeWithFallback(contract.DurationMonths);
            var durationYears = parameters.DurationYears.MapOptionalChangeWithFallback(contract.DurationYears);
            var continuous = parameters.IsContinuous.MapOptionalChangeWithFallback(contract.DurationOngoing);

            return contract
                .UpdateDuration(durationMonths, durationYears, continuous)
                .Match<Result<ItContract, OperationError>>(error => error, () => contract);
        }

        private Maybe<OperationError> UpdateExtensionOption(ItContract contract, Guid? extensionOptionUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                extensionOptionUuid,
                itContract => itContract.ResetExtensionOption(),
                itContract => itContract.OptionExtend,
                (itContract, newOption) => itContract.OptionExtend = newOption
            );
        }

        private Maybe<OperationError> UpdateRoles(ItContract contract, IEnumerable<UserRolePair> input)
        {
            var roleAssignments = input
                .Select(pair => (pair.RoleUuid, pair.UserUuid))
                .ToList();

            return _roleAssignmentService
                .BatchUpdateRoles(contract, roleAssignments)
                .Select(error => new OperationError($"Failed while updating role assignments:{error.Message.GetValueOrEmptyString()}", error.FailureType));
        }

        private Result<ItContract, OperationError> UpdateTermination(ItContract contract, ItContractTerminationParameters termination)
        {
            return contract
                .WithOptionalUpdate(termination.TerminatedAt, (c, newValue) => c.Terminated = newValue.HasValue ? newValue.Value : null)
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(termination.NoticePeriodMonthsUuid, UpdateNoticePeriodMonthsUuid))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(termination.NoticePeriodExtendsCurrent, (c, newValue) => c.Running = newValue.HasValue ? newValue.Value : null))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(termination.NoticeByEndOf, (c, newValue) => c.ByEnding = newValue.HasValue ? newValue.Value : null));
        }

        private Maybe<OperationError> UpdateNoticePeriodMonthsUuid(ItContract contract, Guid? optionUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                optionUuid,
                itContract  => itContract.ResetNoticePeriod(),
                itContract => itContract.TerminationDeadline,
                (itContract, newValue) => itContract.TerminationDeadline = newValue
            );
        }

        private Maybe<OperationError> UpdateExternalReferences(ItContract contract, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            return _referenceService
                .BatchUpdateExternalReferences(
                    ReferenceRootType.Contract,
                    contract.Id,
                    externalReferences.ToList())
                .Select(error => new OperationError($"Failed to update references with error message: {error.Message.GetValueOrEmptyString()}", error.FailureType));
        }

        private Maybe<OperationError> UpdateHandOverTrials(ItContract contract, IEnumerable<ItContractHandoverTrialUpdate> parameters)
        {
            var handoverTrialTypes = new Dictionary<Guid, HandoverTrialType>();
            var updates = parameters.ToList();
            foreach (var uuid in updates.Select(x => x.HandoverTrialTypeUuid).Distinct().ToList())
            {
                var optionType = _optionResolver.GetOptionType<HandoverTrial, HandoverTrialType>(contract.Organization.Uuid, uuid);
                if (optionType.Failed)
                    return new OperationError($"Failed to fetch option with uuid:{uuid}. Message:{optionType.Error.Message.GetValueOrEmptyString()}", optionType.Error.FailureType);

                if (!optionType.Value.available && contract.HandoverTrials.Any(x => x.HandoverTrialType.Uuid == uuid) == false)
                    return new OperationError($"Cannot take new handover trial ({uuid}) into use which is not available in the organization", OperationFailure.BadInput);

                handoverTrialTypes[uuid] = optionType.Value.option;
            }

            //Replace existing trials (duplicates are allowed so we cannot derive any meaningful unique identity)
            var handoverTrials = contract.HandoverTrials.ToList();
            handoverTrials.ForEach(_handoverTrialRepository.Delete);
            contract.ResetHandoverTrials();

            foreach (var newHandoverTrial in updates)
            {
                var error = contract.AddHandoverTrial(handoverTrialTypes[newHandoverTrial.HandoverTrialTypeUuid], newHandoverTrial.ExpectedAt, newHandoverTrial.ApprovedAt);
                if (error.HasValue)
                    return new OperationError("Failed adding handover trial:" + error.Value.Message.GetValueOrEmptyString(), error.Value.FailureType);
            }

            return Maybe<OperationError>.None;
        }

        private Result<ItContract, OperationError> UpdateSupplierData(ItContract contract, ItContractSupplierModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.OrganizationUuid, UpdateSupplierOrganization)
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.Signed, (c, newValue) => c.HasSupplierSigned = newValue))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.SignedAt, (c, newValue) => c.SupplierSignedDate = newValue?.Date))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.SignedBy, (c, newValue) => c.SupplierContractSigner = newValue));
        }

        private Maybe<OperationError> UpdateSupplierOrganization(ItContract contract, Guid? organizationId)
        {
            if (!organizationId.HasValue)
            {
                contract.ResetSupplierOrganization();
            }
            else
            {
                var organizationResult = _organizationService.GetOrganization(organizationId.Value);
                if (organizationResult.Failed)
                {
                    return new OperationError($"Failed to get supplier organization:{organizationResult.Error.Message.GetValueOrEmptyString()}", organizationResult.Error.FailureType);
                }

                return contract.SetSupplierOrganization(organizationResult.Value);
            }
            return Maybe<OperationError>.None;
        }

        private static Result<ItContract, OperationError> UpdateResponsibleData(ItContract contract, ItContractResponsibleDataModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.OrganizationUnitUuid, UpdateResponsibleOrganizationUnit)
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.Signed, (c, newValue) => c.IsSigned = newValue))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.SignedAt, (c, newValue) => c.SignedDate = newValue?.Date))
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.SignedBy, (c, newValue) => c.ContractSigner = newValue));
        }

        private static Maybe<OperationError> UpdateResponsibleOrganizationUnit(ItContract contract, Guid? organizationUnitUuid)
        {
            if (organizationUnitUuid.HasValue)
            {
                return contract.SetResponsibleOrganizationUnit(organizationUnitUuid.Value);
            }
            contract.ResetResponsibleOrganizationUnit();

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdateDataProcessingRegistrations(ItContract contract, IEnumerable<Guid> dataProcessingRegistrationUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
            (
                "data processing registration",
                contract,
                dataProcessingRegistrationUuids.FromNullable(),
                (dprUuid) => _dataProcessingRegistrationApplicationService.GetByUuid(dprUuid),
                itContract => itContract.DataProcessingRegistrations.ToList(),
                (itContract, registration) => itContract.AssignDataProcessingRegistration(registration).MatchFailure(),
                (itContract, registration) => itContract.RemoveDataProcessingRegistration(registration).MatchFailure()
            );
        }

        private Result<ItContract, OperationError> UpdatePaymentModelParameters(ItContract contract, ItContractPaymentModelModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.OperationsRemunerationStartedAt, (c, newValue) => c.OperationRemunerationBegun = newValue.Match(val => val, () => (DateTime?)null))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PaymentFrequencyUuid, UpdatePaymentFrequency))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PaymentModelUuid, UpdatePaymentModel))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PriceRegulationUuid, UpdatePriceRegulation))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PaymentMileStones, UpdatePaymentMileStones));
        }

        private Maybe<OperationError> UpdatePaymentMileStones(ItContract contract, IEnumerable<ItContractPaymentMilestone> milestones)
        {
            //Replace existing milestones (duplicates are allowed so we cannot derive any meaningful unique identity)
            var paymentMilestones = contract.PaymentMilestones.ToList();
            contract.ResetPaymentMilestones();
            paymentMilestones.ForEach(_paymentMilestoneRepository.Delete);

            foreach (var newMilestone in milestones)
            {
                var error = contract.AddPaymentMilestone(newMilestone.Title, newMilestone.Expected, newMilestone.Approved);
                if (error.HasValue)
                    return new OperationError($"Failed adding payment milestone: {error.Value.Message.GetValueOrEmptyString()}", error.Value.FailureType);
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> UpdatePriceRegulation(ItContract contract, Guid? priceRegulationUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                priceRegulationUuid,
                c => c.ResetPriceRegulation(),
                c => c.PriceRegulation,
                (c, newValue) => c.PriceRegulation = newValue
            );
        }

        private Maybe<OperationError> UpdatePaymentModel(ItContract contract, Guid? paymentModelUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                paymentModelUuid,
                c => c.ResetPaymentModel(),
                c => c.PaymentModel,
                (c, newValue) => c.PaymentModel = newValue
            );
        }

        private Maybe<OperationError> UpdatePaymentFrequency(ItContract contract, Guid? paymentFrequencyUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                paymentFrequencyUuid,
                c => c.ResetPaymentFrequency(),
                c => c.PaymentFreqency,
                (c, newValue) => c.PaymentFreqency = newValue
            );
        }

        private Maybe<OperationError> UpdateSystemAssignments(ItContract contract, IEnumerable<Guid> systemUsageUuids)
        {
            return _assignmentUpdateService.UpdateUniqueMultiAssignment
             (
                 "system usage",
                 contract,
                 systemUsageUuids.FromNullable(),
                 (systemUsageUuid) => _usageService.GetByUuid(systemUsageUuid),
                 itContract => itContract.AssociatedSystemUsages.Select(x => x.ItSystemUsage).ToList(),
                 (itContract, usage) => itContract.AssignSystemUsage(usage),
                 (itContract, usage) => itContract.RemoveSystemUsage(usage)
             );
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
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
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
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
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

            return contract
                .UpdateProcurementPlan(plan.Value)
                .Select(error => new OperationError($"Failed to update procurement plan with error message: {error.Message.GetValueOrEmptyString()}", error.FailureType));
        }

        private Maybe<OperationError> UpdatePurchaseType(ItContract contract, Guid? purchaseTypeUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment(
                contract,
                purchaseTypeUuid,
                c => c.ResetPurchaseForm(),
                c => c.PurchaseForm,
                (c, newValue) => c.PurchaseForm = newValue
            );
        }

        private Maybe<OperationError> UpdateProcurementStrategy(ItContract contract, Guid? procurementStrategyUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment(
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
    }
}
