using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Events;
using Core.DomainModel.Organization;
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

        public OrganizationUnitService(IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService, 
            IItContractService contractService,
            IItSystemUsageService usageService, 
            IAuthorizationContext authorizationContext, 
            ITransactionManager transactionManager, 
            IDomainEvents domainEvents,
            IDatabaseControl databaseControl)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _transactionManager = transactionManager;
            _domainEvents = domainEvents;
            _databaseControl = databaseControl;
        }

        public Result<OrganizationRegistrationDetails, OperationError> GetRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            return _organizationService.GetOrganization(organizationUuid, OrganizationDataReadAccessLevel.All)
                .Match<Result<OrganizationUnit, OperationError>>
                (
                    organization =>
                    {
                        var unit = organization.GetOrganizationUnit(unitUuid);
                        if(unit.IsNone)
                            return new OperationError($"Organization unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);

                        return unit.Value;
                    },
                    error => error
                )
                .Match
                (
                    unit => Result<OrganizationRegistrationDetails, OperationError>.Success(unit.GetUnitRegistrations()),
                    error => error
                );
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid, OrganizationRegistrationChangeParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var authorizeResult = GetOrganizationAndAuthorizeModification(organizationUuid);
            if (authorizeResult.Failed)
            {
                return authorizeResult.Error;
            }

            var unit = authorizeResult.Value.GetOrganizationUnit(unitUuid);
            if (unit.IsNone)
            {
                return new OperationError($"Unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowDelete(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            
            var result =  _organizationRightsService.RemoveUnitRightsByIds(organizationUuid, unitUuid, parameters.OrganizationUnitRights)
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
                    () => RemoveSystemRelevantUnits(parameters.RelevantSystems, unit.Value.Id)
                );

            if (result.HasValue)
            {
                return result.Value;
            }

            _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(unit.Value));
            _databaseControl.SaveChanges();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> DeleteRegistrations(Guid organizationUuid, Guid unitUuid)
        {
            using var transaction = _transactionManager.Begin();

            var deleteRegistrationsError = GetRegistrations(organizationUuid, unitUuid)
                .Match(val => DeleteRegistrations(organizationUuid, unitUuid, ToChangeParametersFromRegistrationDetails(val)),
                    error => error);

            if (deleteRegistrationsError.HasValue)
                return deleteRegistrationsError.Value;

            _databaseControl.SaveChanges();
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferRegistrations(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, OrganizationRegistrationChangeParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var organizationResult = GetOrganizationAndAuthorizeModification(organizationUuid);
            if (organizationResult.Failed)
            {
                return organizationResult.Error;
            }

            var unit = organizationResult.Value.GetOrganizationUnit(targetUnitUuid);
            if (unit.IsNone)
            {
                return new OperationError($"Unit with uuid: {unitUuid} was not found", OperationFailure.NotFound);
            }

            var targetUnit = organizationResult.Value.GetOrganizationUnit(targetUnitUuid);
            if (targetUnit.IsNone)
            {
                return new OperationError($"Unit with uuid: {targetUnitUuid} was not found", OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowModify(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            if (!_authorizationContext.AllowModify(targetUnit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var result = _organizationRightsService.TransferUnitRightsByIds(organizationUuid, unitUuid, targetUnitUuid, parameters.OrganizationUnitRights)
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
                    () => TransferSystemResponsibleRegistrations(targetUnit.Value, parameters.ResponsibleSystems)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemRelevantRegistrations(unit.Value.Id, targetUnit.Value, parameters.RelevantSystems)
                );

            if (result.HasValue)
            {
                return result.Value;
            }

            _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(unit.Value));
            _domainEvents.Raise(new EntityUpdatedEvent<OrganizationUnit>(targetUnit.Value));
            _databaseControl.SaveChanges();
            transaction.Commit();
            return Maybe<OperationError>.None;
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
                var deleteError = _contractService.RemoveContractResponsibleUnit(contractId);
                if (deleteError.HasValue)
                    return deleteError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemRelevantUnits(IEnumerable<int> systemIds, int unitId)
        {
            foreach (var systemId in systemIds)
            {
                var deleteError = _usageService.RemoveRelevantUnit(systemId, unitId);
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

        private Maybe<OperationError> TransferSystemResponsibleRegistrations(OrganizationUnit targetUnit, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferResponsibleUsage(targetUnit, systemId);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferContractRegistrations(Guid targetUnitUuid, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var transferError = _contractService.SetContractResponsibleUnit(contractId, targetUnitUuid);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemRelevantRegistrations(int unitId, OrganizationUnit targetUnit, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferError = _usageService.TransferRelevantUsage(unitId, targetUnit, systemId);
                if (transferError.HasValue)
                    return transferError.Value;
            }

            return Maybe<OperationError>.None;
        }

        private static OrganizationRegistrationChangeParameters ToChangeParametersFromRegistrationDetails(
            OrganizationRegistrationDetails registrations)
        {
            var itContractRegistrations = registrations.ItContractRegistrations.Select(x => x.Id).ToList();
            var organizationUnitRights = registrations.OrganizationUnitRights.Select(x => x.Id).ToList();
            var paymentRegistrationDetails = registrations.PaymentRegistrationDetails.Select
            (x =>
                new PaymentChangeParameters
                (
                    x.ItContract.Id,
                    x.InternalPayments.Select(ip => ip.Id).ToList(),
                    x.ExternalPayments.Select(ep => ep.Id).ToList()
                )
            ).ToList();
            var relevantSystems = registrations.RelevantSystems.Select(x => x.Id).ToList();
            var responsibleSystems = registrations.ResponsibleSystems.Select(x => x.Id).ToList();

            return new OrganizationRegistrationChangeParameters(organizationUnitRights, itContractRegistrations, paymentRegistrationDetails, responsibleSystems,relevantSystems);
        }
    }
}
