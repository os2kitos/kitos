using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Authorization;
using Core.DomainServices.Generic;
using Infrastructure.Services.DataAccess;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationUnitService : IOrganizationUnitService
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrgUnitService _orgUnitService;
        private readonly ITransactionManager _transactionManager;

        public OrganizationUnitService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService, 
            IItContractService contractService,
            IItSystemUsageService usageService, 
            IAuthorizationContext authorizationContext, 
            IOrgUnitService orgUnitService,
            ITransactionManager transactionManager)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _orgUnitService = orgUnitService;
            _transactionManager = transactionManager;
            _identityResolver = identityResolver;
        }

        public Result<OrganizationRegistrationDetails, OperationError> GetOrganizationRegistrations(int organizationId, int unitId)
        {
            return GetOrganization(organizationId, OrganizationDataReadAccessLevel.All)
                .Match
                (
                    _ => GetOrganziationUnit(unitId),
                    error => error
                )
                .Match
                (
                    unit => _authorizationContext.AllowReads(unit) == false
                        ? new OperationError("User is not allowed to read the unit", OperationFailure.Forbidden)
                        : Result<OrganizationRegistrationDetails, OperationError>.Success(unit.GetUnitRegistrations()),
                    error => error
                );
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int organizationId, int unitId, OrganizationRegistrationChangeParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var canModifyOrganization = CanModifyOrganization(organizationId);
            if (canModifyOrganization.HasValue)
            {
                return canModifyOrganization.Value;
            }

            var unit = GetOrganziationUnit(unitId);
            if (unit.Failed)
            {
                return unit.Error;
            }

            if (!_authorizationContext.AllowDelete(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            
            var result =  _organizationRightsService.RemoveUnitRightsByIds(organizationId, parameters.OrganizationUnitRights)
                .Match
                (
                    error => error,
                () => RemovePayments(parameters.PaymentRegistrationDetails)
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
                    () => RemoveSystemRelevantUnits(parameters.RelevantSystems, unitId)
                );

            if (result.HasValue)
            {
                return result.Value;
            }

            transaction.Commit();
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> DeleteAllUnitOrganizationRegistrations(int organizationId, int unitId)
        {
            using var transaction = _transactionManager.Begin();

            var canModifyOrganization = CanModifyOrganization(organizationId);
            if (canModifyOrganization.HasValue)
            {
                return canModifyOrganization.Value;
            }

            var deleteRegistrationsResult = GetOrganizationRegistrations(organizationId, unitId)
                .Match(val => DeleteSelectedOrganizationRegistrations(organizationId, unitId, ToChangeParametersFromRegistrationDetails(val)),
                    error => error);

            if (deleteRegistrationsResult.HasValue)
                return deleteRegistrationsResult.Value;

            _orgUnitService.Delete(organizationId, unitId);
            transaction.Commit();

            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations(int organizationId, int unitId, int targetUnitId, OrganizationRegistrationChangeParameters parameters)
        {
            using var transaction = _transactionManager.Begin();

            var canModifyOrganization = CanModifyOrganization(organizationId);
            if (canModifyOrganization.HasValue)
            {
                return canModifyOrganization.Value;
            }

            var unit = GetOrganziationUnit(unitId);
            if (unit.Failed)
            {
                return unit.Error;
            }

            var targetUnit = GetOrganziationUnit(targetUnitId);
            if (targetUnit.Failed)
            {
                return targetUnit.Error;
            }

            if (!_authorizationContext.AllowModify(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            if (!_authorizationContext.AllowModify(targetUnit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var result = _organizationRightsService.TransferUnitRightsByIds(organizationId, targetUnitId, parameters.OrganizationUnitRights)
                .Match
                (
                    error => error,
                    () => TransferPayments(unitId, parameters.PaymentRegistrationDetails)
                )
                .Match
                (
                    error => error,
                    () => TransferContractRegistrations(unitId, parameters.ItContractRegistrations)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemResponsibleRegistrations(targetUnit.Value, parameters.ResponsibleSystems)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemRelevantRegistrations(unitId, targetUnit.Value, parameters.RelevantSystems)
                );

            if (result.HasValue)
            {
                return result.Value;
            }

            transaction.Commit();
            return Maybe<OperationError>.None;
        }

        private Result<OrganizationUnit, OperationError> GetOrganziationUnit(int unitId)
        {
            return _identityResolver.ResolveUuid<OrganizationUnit>(unitId)
                .Match(x => _organizationService.GetOrganizationUnit(x),
                    () => new OperationError($"Organization unit with id: {unitId} not found", OperationFailure.NotFound));
        }

        private Result<Organization, OperationError> GetOrganization(int id, OrganizationDataReadAccessLevel? accessLevel = null)
        {
            return _identityResolver.ResolveUuid<Organization>(id)
                .Match(x => _organizationService.GetOrganization(x, accessLevel),
                    () => new OperationError($"Organization with id: {id} not found", OperationFailure.NotFound));
        }

        private Maybe<OperationError> CanModifyOrganization(int id, OrganizationDataReadAccessLevel? accessLevel = null)
        {
            return GetOrganization(id, accessLevel)
                .Match
                (
                    organization => _authorizationContext.AllowModify(organization) == false 
                        ? new OperationError("User is not allowed to modify the organization", OperationFailure.Forbidden) 
                        : Maybe<OperationError>.None,
                    error => error
                );
        }

        private Maybe<OperationError> RemovePayments(IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var removeInternalPaymentsResult = _contractService.RemovePayments(payment.ItContractId, true, payment.InternalPayments);
                if (removeInternalPaymentsResult.HasValue)
                    return removeInternalPaymentsResult.Value;

                var removeExternalPaymentsResult = _contractService.RemovePayments(payment.ItContractId, false, payment.ExternalPayments);
                if (removeExternalPaymentsResult.HasValue)
                    return removeExternalPaymentsResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveContractRegistrations(IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteResult = _contractService.RemoveContractResponsibleUnit(contractId);
                if (deleteResult.HasValue)
                    return deleteResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemRelevantUnits(IEnumerable<int> systemIds, int unitId)
        {
            foreach (var systemId in systemIds)
            {
                var deleteResult = _usageService.RemoveRelevantUnit(systemId, unitId);
                if (deleteResult.HasValue)
                    return deleteResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemResponsibleRegistrations(IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteResult = _usageService.RemoveResponsibleUsage(systemId);
                if (deleteResult.HasValue)
                    return deleteResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferPayments(int targetUnitId, IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var transferInternalPaymentsResult = _contractService.TransferPayments(payment.ItContractId, targetUnitId, true, payment.InternalPayments);
                if (transferInternalPaymentsResult.HasValue)
                    return transferInternalPaymentsResult.Value;

                var transferExternalPaymentsResult = _contractService.TransferPayments(payment.ItContractId, targetUnitId, false, payment.ExternalPayments);
                if (transferExternalPaymentsResult.HasValue)
                    return transferExternalPaymentsResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemResponsibleRegistrations(OrganizationUnit targetUnit, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferResult = _usageService.TransferResponsibleUsage(targetUnit, systemId);
                if (transferResult.HasValue)
                    return transferResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferContractRegistrations(int targetUnitId, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var transferResult = _contractService.SetContractResponsibleUnit(contractId, targetUnitId);
                if (transferResult.HasValue)
                    return transferResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemRelevantRegistrations(int unitId, OrganizationUnit targetUnit, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var transferResult = _usageService.TransferRelevantUsage(unitId, targetUnit, systemId);
                if (transferResult.HasValue)
                    return transferResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private static OrganizationRegistrationChangeParameters ToChangeParametersFromRegistrationDetails(
            OrganizationRegistrationDetails registrations)
        {
            var itContractRegistrations = registrations.ItContractRegistrations.Select(x => x.Id);
            var organizationUnitRights = registrations.OrganizationUnitRights.Select(x => x.Id);
            var paymentRegistrationDetails = registrations.PaymentRegistrationDetails.Select
            (x =>
                new PaymentChangeParameters
                (
                    x.ItContract.Id,
                    x.InternalPayments.Select(ip => ip.Id),
                    x.ExternalPayments.Select(ep => ep.Id)
                )
            );
            var relevantSystems = registrations.RelevantSystems.Select(x => x.Id);
            var responsibleSystems = registrations.ResponsibleSystems.Select(x => x.Id);

            return new OrganizationRegistrationChangeParameters(itContractRegistrations, organizationUnitRights, paymentRegistrationDetails, responsibleSystems,relevantSystems);
        }
    }
}
