using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Extensions;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Generic;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Model.Contracts.Write;
using Core.ApplicationServices.Model.Shared;
using Core.ApplicationServices.Model.Shared.Write;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
using Core.DomainModel.References;
using Core.DomainModel.Shared;
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
        private readonly IReferenceService _referenceService;
        private readonly IAssignmentUpdateService _assignmentUpdateService;
        private readonly IItSystemUsageService _usageService;
        private readonly IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> _roleAssignmentService;
        private readonly IDataProcessingRegistrationApplicationService _dataProcessingRegistrationApplicationService;
        private readonly IGenericRepository<EconomyStream> _economyStreamRepository;
        private readonly IEntityTreeUuidCollector _entityTreeUuidCollector;

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
            IReferenceService referenceService,
            IAssignmentUpdateService assignmentUpdateService,
            IItSystemUsageService usageService,
            IRoleAssignmentService<ItContractRight, ItContractRole, ItContract> roleAssignmentService,
            IDataProcessingRegistrationApplicationService dataProcessingRegistrationApplicationService,
            IGenericRepository<EconomyStream> economyStreamRepository,
            IEntityTreeUuidCollector entityTreeUuidCollector)
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
            _referenceService = referenceService;
            _assignmentUpdateService = assignmentUpdateService;
            _usageService = usageService;
            _roleAssignmentService = roleAssignmentService;
            _dataProcessingRegistrationApplicationService = dataProcessingRegistrationApplicationService;
            _economyStreamRepository = economyStreamRepository;
            _entityTreeUuidCollector = entityTreeUuidCollector;
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

        public Maybe<OperationError> Delete(Guid itContractUuid)
        {
            var dbId = _entityIdentityResolver.ResolveDbId<ItContract>(itContractUuid);

            if (dbId.IsNone)
                return new OperationError("Invalid contract uuid", OperationFailure.NotFound);

            return _contractService
                .Delete(dbId.Value)
                .Match(_ => Maybe<OperationError>.None, failure => new OperationError("Failed deleting contract", failure));
        }

        public Result<ExternalReference, OperationError> AddExternalReference(Guid contractUuid, ExternalReferenceProperties externalReferenceProperties)
        {
            return GetContractAndAuthorizeAccess(contractUuid)
                .Bind(usage => _referenceService.AddReference(usage.Id, ReferenceRootType.Contract, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> UpdateExternalReference(Guid contractUuid, Guid externalReferenceUuid,
            ExternalReferenceProperties externalReferenceProperties)
        {
            return GetContractAndAuthorizeAccess(contractUuid)
                .Bind(usage => _referenceService.UpdateReference(usage.Id, ReferenceRootType.Contract, externalReferenceUuid, externalReferenceProperties));
        }

        public Result<ExternalReference, OperationError> DeleteExternalReference(Guid contractUuid, Guid externalReferenceUuid)
        {
            return GetContractAndAuthorizeAccess(contractUuid)
                .Bind(_ =>
                {
                    var getIdResult = _entityIdentityResolver.ResolveDbId<ExternalReference>(externalReferenceUuid);
                    if (getIdResult.IsNone)
                        return new OperationError($"ExternalReference with uuid: {externalReferenceUuid} was not found", OperationFailure.NotFound);
                    var externalReferenceId = getIdResult.Value;

                    return _referenceService.DeleteByReferenceId(externalReferenceId)
                        .Match(Result<ExternalReference, OperationError>.Success,
                            operationFailure =>
                                new OperationError($"Failed to remove the ExternalReference with uuid: {externalReferenceUuid}", operationFailure));
                });
        }

        public Result<ItContract, OperationError> AddRole(Guid contractUuid, UserRolePair assignment)
        {
            return _contractService
                .GetContract(contractUuid)
                .Select(ExtractAssignedRoles)
                .Bind<ItContractModificationParameters>(existingRoles =>
                {
                    if (existingRoles.Contains(assignment))
                    {
                        return new OperationError("Role assignment exists", OperationFailure.Conflict);
                    }
                    return CreateRoleAssignmentUpdate(existingRoles.Append(assignment));
                })
                .Bind(update => Update(contractUuid, update));
        }

        public Result<ItContract, OperationError> RemoveRole(Guid systemUsageUuid, UserRolePair assignment)
        {
            return _contractService
                .GetContract(systemUsageUuid)
                .Select(ExtractAssignedRoles)
                .Bind<ItContractModificationParameters>(existingRoles =>
                {
                    if (!existingRoles.Contains(assignment))
                    {
                        return new OperationError("Assignment does not exist", OperationFailure.BadInput);
                    }
                    return CreateRoleAssignmentUpdate(existingRoles.Except(assignment.WrapAsEnumerable()));
                })
                .Bind(update => Update(systemUsageUuid, update));
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
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.PaymentModel, UpdatePaymentModelParameters))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.ExternalReferences, UpdateExternalReferences))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Roles, UpdateRoles))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.AgreementPeriod, UpdateAgreementPeriod))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Payments, UpdatePayments))
                .Bind(updateContract => updateContract.WithOptionalUpdate(parameters.Termination, UpdateTermination));
        }

        private Result<ItContract, OperationError> UpdatePayments(ItContract contract, ItContractPaymentDataModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.ExternalPayments, UpdateExternalPayments)
                .Bind(updatedContract => updatedContract.WithOptionalUpdate(parameters.InternalPayments, UpdateInternalPayments));
        }

        private Maybe<OperationError> UpdateInternalPayments(ItContract contract, IEnumerable<ItContractPayment> newPaymentState)
        {
            return ReplacePayments(true, contract.InternEconomyStreams.ToList(), newPaymentState, contract.ResetInternalEconomyStreams, contract.AddInternalEconomyStream);
        }

        private Maybe<OperationError> UpdateExternalPayments(ItContract contract, IEnumerable<ItContractPayment> newPaymentState)
        {
            return ReplacePayments(false, contract.ExternEconomyStreams.ToList(), newPaymentState, contract.ResetExternalEconomyStreams, contract.AddExternalEconomyStream);
        }

        private delegate Maybe<OperationError> AddPaymentFunc(Guid? optionalOrganizationUnitUuid, int acquisition, int operation, int other, string accountingEntry, TrafficLight auditStatus, DateTime? auditDate, string note);

        private Maybe<OperationError> ReplacePayments(
            bool internalPayment,
            IEnumerable<EconomyStream> currentState,
            IEnumerable<ItContractPayment> newPaymentState,
            Action resetCurrentState,
            AddPaymentFunc addPayment)
        {
            var subject = internalPayment ? "internal" : "external";
            var economyStreams = currentState.ToList();
            resetCurrentState();
            economyStreams.ForEach(_economyStreamRepository.Delete);

            foreach (var itContractPayment in newPaymentState.ToList())
            {
                var error = addPayment(itContractPayment.OrganizationUnitUuid, itContractPayment.Acquisition, itContractPayment.Operation, itContractPayment.Other, itContractPayment.AccountingEntry, itContractPayment.AuditStatus, itContractPayment.AuditDate, itContractPayment.Note);
                if (error.HasValue)
                    return new OperationError($"Failed to add {subject} payment:{error.Value.Message.GetValueOrEmptyString()}", error.Value.FailureType);
            }

            return Maybe<OperationError>.None;
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
                itContract => itContract.ResetNoticePeriod(),
                itContract => itContract.TerminationDeadline,
                (itContract, newValue) => itContract.TerminationDeadline = newValue
            );
        }

        private Maybe<OperationError> UpdateExternalReferences(ItContract contract, IEnumerable<UpdatedExternalReferenceProperties> externalReferences)
        {
            return _referenceService
                .UpdateExternalReferences(
                    ReferenceRootType.Contract,
                    contract.Id,
                    externalReferences.ToList())
                .Select(error => new OperationError($"Failed to update references with error message: {error.Message.GetValueOrEmptyString()}", error.FailureType));
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
                GetAssignmentInputFromKey,
                itContract => itContract.DataProcessingRegistrations.ToList(),
                (itContract, registration) => _contractService.AssignDataProcessingRegistration(itContract.Id, registration.Id).MatchFailure(),
                (itContract, registration) => _contractService.RemoveDataProcessingRegistration(itContract.Id, registration.Id).MatchFailure()
            );
        }

        private Result<DataProcessingRegistration, OperationError> GetAssignmentInputFromKey(Guid dprUuid)
        {
            return _dataProcessingRegistrationApplicationService.GetByUuid(dprUuid);
        }

        private Result<ItContract, OperationError> UpdatePaymentModelParameters(ItContract contract, ItContractPaymentModelModificationParameters parameters)
        {
            return contract
                .WithOptionalUpdate(parameters.OperationsRemunerationStartedAt, (c, newValue) => c.OperationRemunerationBegun = newValue.Match(val => val, () => (DateTime?)null))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PaymentFrequencyUuid, UpdatePaymentFrequency))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PaymentModelUuid, UpdatePaymentModel))
                .Bind(itContract => itContract.WithOptionalUpdate(parameters.PriceRegulationUuid, UpdatePriceRegulation));
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
                 (systemUsageUuid) => _usageService.GetReadableItSystemUsageByUuid(systemUsageUuid),
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
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.AgreementElementUuids, UpdateAgreementElements))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.CriticalityUuid, UpdateContractCriticality))
                .Bind(itContract => itContract.WithOptionalUpdate(generalData.RequireValidParent, UpdateRequireValidParent));
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

                var (option, available) = result.Value;
                if (available == false && contract.AssociatedAgreementElementTypes.Any(x => x.AgreementElementType.Uuid == uuid) == false)
                {
                    return new OperationError($"Tried to add agreement element which is not available in the organization: {uuid}", OperationFailure.BadInput);
                }
                agreementElementTypes.Add(option);
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
                .Bind(itContract =>
                    itContract.WithOptionalUpdate(procurementParameters.PurchaseTypeUuid, UpdatePurchaseType))
                .Bind(itContract =>
                    itContract.WithOptionalUpdate(procurementParameters.ProcurementPlan, UpdateProcurementPlan))
                .Bind(itContract => itContract.WithOptionalUpdate(procurementParameters.ProcurementInitiated,
                    (c, newValue) => c.ProcurementInitiated = newValue.GetValueOrFallback(YesNoUndecidedOption.Undecided)));
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

            var contractAndChildrenUuids = _entityTreeUuidCollector.CollectSelfAndDescendantUuids(contract);
            if (contractAndChildrenUuids.Contains(newParentUuid))
            {
                return new OperationError($"Failed to set parent with Uuid: {newParentUuid.Value} because it is identical to or a descendant of contract with Uuid: {contract.Uuid}", OperationFailure.BadInput);
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

        private Maybe<OperationError> UpdateRequireValidParent(ItContract itContract, Maybe<bool> requireValidParent)
        {
            if (requireValidParent.IsNone) return Maybe<OperationError>.None;
            return itContract.SetRequireValidParent(requireValidParent.Value);
        }

        private Maybe<OperationError> UpdateName(ItContract contract, string newName)
        {
            var error = _contractService.ValidateNewName(contract.Id, newName);

            if (error.HasValue)
                return error;

            contract.Name = newName;
            return Maybe<OperationError>.None;
        }
        private Maybe<OperationError> UpdateContractCriticality(ItContract contract, Guid? criticalityUuid)
        {
            return _assignmentUpdateService.UpdateIndependentOptionTypeAssignment
            (
                contract,
                criticalityUuid,
                c => c.ResetCriticality(),
                c => c.Criticality,
                (c, newValue) => c.Criticality = newValue
            );
        }

        private Result<ItContract, OperationError> GetContractAndAuthorizeAccess(Guid contractUuid)
        {
            return _contractService
                .GetContract(contractUuid)
                .Bind(WithWriteAccess);
        }

        private static IReadOnlyList<UserRolePair> ExtractAssignedRoles(ItContract contract)
        {
            return contract.Rights.Select(right => new UserRolePair(right.User.Uuid, right.Role.Uuid)).ToList();
        }

        private static ItContractModificationParameters CreateRoleAssignmentUpdate(IEnumerable<UserRolePair> existingRoles)
        {
            return new ItContractModificationParameters
            {
                Roles = existingRoles.FromNullable()
            };
        }
    }
}
