using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Generic;

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

        public OrganizationUnitService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService, 
            IItContractService contractService,
            IItSystemUsageService usageService, 
            IAuthorizationContext authorizationContext, 
            IOrgUnitService orgUnitService)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _orgUnitService = orgUnitService;
            _identityResolver = identityResolver;
        }

        public Result<OrganizationRegistrationDetails, OperationError> GetOrganizationRegistrations(int unitId)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }
            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            if (unit.Failed)
            {
                return new OperationError("Organization not found", OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowReads(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            var details = new OrganizationRegistrationDetails
            {
                OrganizationUnitRights = unit.Value.Rights,
                ItContractRegistrations = _contractService.GetContractsByResponsibleUnitId(unitId).ToList(),
                PaymentRegistrationDetails = _contractService.GetContractsWhereUnitIsResponsibleForPayment(unitId)
                    .Select(itContract => new PaymentRegistrationDetails(unitId, itContract)).ToList(),
                ResponsibleSystems = _usageService.GetSystemsByResponsibleUnitId(unitId).ToList(),
                RelevantSystems = _usageService.GetSystemsByRelevantUnitId(unitId).ToList()
            };

            return details;
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, OrganizationRegistrationChangeParameters parameters)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }
            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            if (unit.Failed)
            {
                return new OperationError("Organization not found", OperationFailure.NotFound);
            }
            if (!_authorizationContext.AllowDelete(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            
            return _organizationRightsService.RemoveUnitRightsByIds(parameters.OrganizationUnitRights)
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
        }

        public Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId, int organizationId)
        {
            var deleteRegistrationsResult = GetOrganizationRegistrations(unitId)
                .Match(val => DeleteSelectedOrganizationRegistrations(unitId, ToChangeParametersFromRegistrationDetails(val)),
                    error => error);

            if (deleteRegistrationsResult.HasValue)
                return deleteRegistrationsResult.Value;

            _orgUnitService.Delete(unitId, organizationId);
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, OrganizationRegistrationChangeParameters parameters)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }
            var targetUnitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(targetUnitId);
            if (targetUnitUuid.IsNone)
            {
                return new OperationError("Target organization id is invalid", OperationFailure.BadInput);
            }

            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            if (unit.Failed)
            {
                return new OperationError("Organization not found", OperationFailure.NotFound);
            }
            var targetUnit = _organizationService.GetOrganizationUnit(targetUnitUuid.Value);
            if (targetUnit.Failed)
            {
                return new OperationError("Target organization not found", OperationFailure.NotFound);
            }

            if (!_authorizationContext.AllowModify(unit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }
            if (!_authorizationContext.AllowModify(targetUnit.Value))
            {
                return new OperationError(OperationFailure.Forbidden);
            }

            return _organizationRightsService.TransferUnitRightsByIds(targetUnitId, parameters.OrganizationUnitRights)
                .Match
                (
                    error => error,
                    () => TransferPayments(targetUnit.Value, parameters.PaymentRegistrationDetails)
                )
                .Match
                (
                    error => error,
                    () => TransferContractRegistrations(targetUnitUuid.Value, parameters.ItContractRegistrations)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemRelevantRegistrations(unitId, targetUnit.Value, parameters.RelevantSystems)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemResponsibleRegistrations(targetUnit.Value, parameters.ResponsibleSystems)
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

        private Maybe<OperationError> TransferPayments(OrganizationUnit targetUnit, IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var transferInternalPaymentsResult = _contractService.TransferPayments(payment.ItContractId, targetUnit, payment.InternalPayments);
                if (transferInternalPaymentsResult.HasValue)
                    return transferInternalPaymentsResult.Value;

                var transferExternalPaymentsResult = _contractService.TransferPayments(payment.ItContractId, targetUnit, payment.ExternalPayments);
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

        private Maybe<OperationError> TransferContractRegistrations(Guid targetUnitUuid, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var transferResult = _contractService.TransferContractResponsibleUnit(targetUnitUuid, contractId);
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
            return new OrganizationRegistrationChangeParameters()
            {
                ItContractRegistrations = registrations.ItContractRegistrations.Select(x => x.Id),
                OrganizationUnitRights = registrations.OrganizationUnitRights.Select(x => x.Id),
                PaymentRegistrationDetails = registrations.PaymentRegistrationDetails.Select
                (x =>
                    new PaymentChangeParameters
                    (
                        x.ItContract.Id,
                        x.InternalPayments.Select(ip => ip.Id),
                        x.ExternalPayments.Select(ep => ep.Id)
                    )
                ),
                RelevantSystems = registrations.RelevantSystems.Select(x => x.Id),
                ResponsibleSystems = registrations.ResponsibleSystems.Select(x => x.Id)
            };
        }
    }
}
