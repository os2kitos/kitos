using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Commands;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationUnitService : IOrganizationUnitService
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly ITransactionManager _transactionManager;
        private readonly IDomainEvents _domainEvents;
        private readonly IDatabaseControl _databaseControl;
        private readonly IGenericRepository<OrganizationUnit> _repository;
        private readonly ICommandBus _commandBus;

        public OrganizationUnitService(IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService,
            IItContractService contractService,
            IItSystemUsageService usageService,
            IAuthorizationContext authorizationContext,
            ITransactionManager transactionManager,
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl,
            IGenericRepository<OrganizationUnit> repository,
            ICommandBus commandBus)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
            _repository = repository;
            _commandBus = commandBus;
        }

        public Result<UnitAccessRights, OperationError> GetAccessRights(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind<(Organization organization,OrganizationUnit organizationUnit)>
                (
                    organization =>
                    {
                        var unit = organization.GetOrganizationUnit(unitUuid);
                        if (unit.IsNone)
                        {
                            return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
                        }
                        return (organization,unit.Value);
                    }
                )
                .Select(orgAndUnit => GetAccessRights(orgAndUnit.organization, orgAndUnit.organizationUnit));
        }

        public Maybe<OperationError> Delete(Guid organizationUuid, Guid unitUuid)
        {
            using var transaction = _transactionManager.Begin();
            var deleteResult = GetOrganizationAndAuthorizeModification(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind(organization => CombineWithOrganizationUnit(unitUuid, organization))
                .Bind(WithDeletionPermission)
                .Bind(DeleteOrganizationUnit);

            if (deleteResult.Failed)
            {
                transaction.Rollback();
            }
            else
            {
                _domainEvents.Raise(new EntityBeingDeletedEvent<OrganizationUnit>(deleteResult.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }
            return deleteResult.MatchFailure();
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid, OrganizationUnitRegistrationChangeParameters parameters)
        {
            return Modify(organizationUuid, unitUuid, (_, unit) =>
            {
                return _organizationRightsService.RemoveUnitRightsByIds(organizationUuid, unitUuid, parameters.OrganizationUnitRights)
                    .Match
                    (
                        error => error,
                        () => RemovePaymentResponsibleUnits(parameters.PaymentRegistrationDetails)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveContractRegistrations(parameters.ItContractRegistrations)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveSystemResponsibleRegistrations(parameters.ResponsibleSystems)
                    )
                    .Match
                    (
                        error => error,
                        () => RemoveSystemRelevantUnits(parameters.RelevantSystems, unitUuid)
                    )
                    .Match
                    (
                        error => error,
                        () => Result<OrganizationUnit, OperationError>.Success(unit)
                    );
            }).MatchFailure();
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return Modify(organizationUuid, unitUuid, (_, unit) =>
            {
                return GetRegistrations(organizationUuid, unitUuid)
                    .Bind<OrganizationUnit>
                    (
                        details =>
                        {
                            var error = DeleteRegistrations(organizationUuid, unitUuid, ToChangeParametersFromRegistrationDetails(details));
                            if (error.HasValue)
                            {
                                return error.Value;
                            }

                            return unit;
                        }
                    );
            }).MatchFailure();
        }

        public Maybe<OperationError> TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, OrganizationUnitRegistrationChangeParameters parameters)
        {
            return Modify(organizationUuid, unitUuid, (organization, unit) =>
            {
                var targetUnitResult = organization.GetOrganizationUnit(targetUnitUuid);
                if (targetUnitResult.IsNone)
                {
                    return new OperationError($"Unit with uuid: {targetUnitUuid} was not found",
                        OperationFailure.NotFound);
                }

                var targetUnit = targetUnitResult.Value;

                if (!_authorizationContext.AllowModify(targetUnit))
                {
                    return new OperationError(OperationFailure.Forbidden);
                }

                var error = _organizationRightsService.TransferUnitRightsByIds(organizationUuid, unitUuid,
                        targetUnitUuid, parameters.OrganizationUnitRights)
                    .Match
                    (
                        error => error,
                        () => TransferPayments(targetUnitUuid, parameters.PaymentRegistrationDetails)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferContractRegistrations(targetUnitUuid, parameters.ItContractRegistrations)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferSystemResponsibleRegistrations(targetUnitUuid, parameters.ResponsibleSystems)
                    )
                    .Match
                    (
                        error => error,
                        () => TransferSystemRelevantRegistrations(unitUuid, targetUnitUuid, parameters.RelevantSystems)
                    );

                if (error.HasValue)
                {
                    return error.Value;
                }

                _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(targetUnitResult.Value));

                return Result<OrganizationUnit, OperationError>.Success(unit);
            }).MatchFailure();
        }

        private Result<TSuccess, OperationError> Modify<TSuccess>(Guid organizationId, Guid unitUuid, Func<Organization, OrganizationUnit, Result<TSuccess, OperationError>> mutation)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationAndAuthorizeModification(organizationId);

            if (organizationResult.Failed)
            {
                return organizationResult.Error;
            }
            var organization = organizationResult.Value;

            var unitResult = organization.GetOrganizationUnit(unitUuid);
            if (unitResult.IsNone)
            {
                return new OperationError($"Unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }
            var unit = unitResult.Value;

            if (!_authorizationContext.AllowModify(unit))
                return new OperationError(OperationFailure.Forbidden);

            var mutationResult = mutation(organization, unit);

            if (mutationResult.Failed)
            {
                transaction.Rollback();
            }
            else
            {
                _repository.Update(unit);
                _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(unitResult.Value));
                _databaseControl.SaveChanges();
                transaction.Commit();
            }

            return mutationResult;
        }

        private Result<Organization, OperationError> GetOrganizationAndAuthorizeModification(Guid uuid, OrganizationDataReadAccessLevel? accessLevel = null)
        {
            return _organizationService.GetOrganization(uuid, accessLevel)
                .Match
                (
                    organization =>
                        _authorizationContext.AllowModify(organization) == false
                            ? new OperationError("User is not allowed to modify the organization", OperationFailure.Forbidden)
                            : Result<Organization, OperationError>.Success(organization),
                    error => error
                );
        }

        private UnitAccessRights GetAccessRights(Organization organization, OrganizationUnit unit)
        {
            var canBeModified = false;
            var canBeRenamed = false;
            var canFieldsBeModified = false;
            var canBeRearranged = false;
            var canBeDeleted = false;

            if (!_authorizationContext.AllowModify(unit))
                return new UnitAccessRights(canBeRead: true, canBeModified, canBeRenamed, canFieldsBeModified, canBeRearranged, canBeDeleted);

            canBeModified = true;
            if (unit.IsOfKitosOrigin())
            {
                canBeRenamed = true;
                canFieldsBeModified = true;

                if (organization.GetRoot() != unit)
                {
                    canBeRearranged = true;
                    canBeDeleted = true;
                }
            }
            if (!_authorizationContext.AllowDelete(unit))
            {
                canBeDeleted = false;
            }

            return new UnitAccessRights(canBeRead: true, canBeModified, canBeRenamed, canFieldsBeModified, canBeRearranged, canBeDeleted);
        }

        private Maybe<OperationError> RemovePaymentResponsibleUnits(IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var removeInternalPaymentsError = _contractService.RemovePaymentResponsibleUnits(payment.ItContractId, true, payment.InternalPayments);
                if (removeInternalPaymentsError.HasValue)
                    return removeInternalPaymentsError.Value;

                var removeExternalPaymentsError = _contractService.RemovePaymentResponsibleUnits(payment.ItContractId, false, payment.ExternalPayments);
                if (removeExternalPaymentsError.HasValue)
                    return removeExternalPaymentsError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveContractRegistrations(IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteError = _contractService.RemoveResponsibleUnit(contractId);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemRelevantUnits(IEnumerable<int> systemIds, Guid unitUuid)
        {
            foreach (var systemId in systemIds)
            {
                var deleteError = _usageService.RemoveRelevantUnit(systemId, unitUuid);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemResponsibleRegistrations(IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteError = _usageService.RemoveResponsibleUsage(systemId);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferPayments(Guid targetUnitUuid, IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var transferInternalPaymentsError = _contractService.TransferPayments(payment.ItContractId, targetUnitUuid, true, payment.InternalPayments);
                if (transferInternalPaymentsError.HasValue)
                    return transferInternalPaymentsError.Value;

                var transferExternalPaymentsError = _contractService.TransferPayments(payment.ItContractId, targetUnitUuid, false, payment.ExternalPayments);
                if (transferExternalPaymentsError.HasValue)
                    return transferExternalPaymentsError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemResponsibleRegistrations(Guid targetUnitUuid, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferResponsibleUsage(systemId, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferContractRegistrations(Guid targetUnitUuid, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var transferError = _contractService.SetResponsibleUnit(contractId, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemRelevantRegistrations(Guid unitUuid, Guid targetUnitUuid, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferRelevantUsage(systemId, unitUuid, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private static OrganizationUnitRegistrationChangeParameters ToChangeParametersFromRegistrationDetails(
            OrganizationUnitRegistrationDetails unitRegistrations)
        {
            var itContractRegistrations = unitRegistrations.ItContractRegistrations.Select(x => x.Id).ToList();
            var organizationUnitRights = unitRegistrations.OrganizationUnitRights.Select(x => x.Id).ToList();
            var paymentRegistrationDetails = unitRegistrations.PaymentRegistrationDetails.Select
            (x =>
                new PaymentChangeParameters
                (
                    x.ItContract.Id,
                    x.InternalPayments.Select(ip => ip.Id).ToList(),
                    x.ExternalPayments.Select(ep => ep.Id).ToList()
                )
            ).ToList();
            var relevantSystems = unitRegistrations.RelevantSystems.Select(x => x.Id).ToList();
            var responsibleSystems = unitRegistrations.ResponsibleSystems.Select(x => x.Id).ToList();

            return new OrganizationUnitRegistrationChangeParameters(organizationUnitRights, itContractRegistrations, paymentRegistrationDetails, responsibleSystems, relevantSystems);
        }

        private Result<OrganizationUnit, OperationError> DeleteOrganizationUnit((Organization organization, OrganizationUnit organizationUnit) orgAndUnit)
        {
            var (organization, organizationUnit) = orgAndUnit;
            var deleteCommand = new RemoveOrganizationUnitRegistrationsCommand(organization, organizationUnit);
            var deleteRegistrationsError = _commandBus.Execute<RemoveOrganizationUnitRegistrationsCommand, Maybe<OperationError>>(deleteCommand);
            if (deleteRegistrationsError.HasValue)
            {
                return deleteRegistrationsError.Value;
            }

            var error = organization.DeleteOrganizationUnit(organizationUnit);
            if (error.HasValue)
            {
                return error.Value;
            }

            _repository.DeleteWithReferencePreload(organizationUnit);
            return organizationUnit;
        }

        private Result<(Organization organization, OrganizationUnit organizationUnit), OperationError> WithDeletionPermission((Organization organization, OrganizationUnit organizationUnit) orgAndUnit)
        {
            var accessRights = GetAccessRights(orgAndUnit.organization, orgAndUnit.organizationUnit);
            if(accessRights.CanBeDeleted == false)
                return new OperationError("Not authorized to delete org unit", OperationFailure.Forbidden);

            return orgAndUnit;
        }

        private static Result<(Organization organization, OrganizationUnit organizationUnit), OperationError> CombineWithOrganizationUnit(Guid unitUuid, Organization organization)
        {
            var organizationUnit = organization.GetOrganizationUnit(unitUuid);
            if (organizationUnit.IsNone)
                return new OperationError(OperationFailure.NotFound);
            return (organization, organizationUnit.Value);
        }

        public Result<OrganizationUnitRegistrationDetails, OperationError> GetRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationService
                .GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Bind<OrganizationUnit>
                (
                    organization =>
                    {
                        var unit = organization.GetOrganizationUnit(unitUuid);
                        if (unit.IsNone)
                            return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);

                        return unit.Value;
                    }
                )
                .Select(unit => unit.GetUnitRegistrations());
        }
    }
}
