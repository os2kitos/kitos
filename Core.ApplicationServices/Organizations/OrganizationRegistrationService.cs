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
    public class OrganizationRegistrationService : IOrganizationRegistrationService
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRegistrationService(IEntityIdentityResolver identityResolver, 
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
                ResponsibleSystems = _usageService.GetSystemsByResponsibleUnitId(unitId).ToList(),
                RelevantSystems = _usageService.GetSystemsByRelevantUnitId(unitId).ToList()
            };
            details.PaymentRegistrationDetails = details.ItContractRegistrations.Select(itContract => new PaymentRegistrationDetails(unitId, itContract)).ToList();

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

        public Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId)
        {
            /*var deleteRegistrationsResult = GetOrganizationRegistrations(unitId)
                .Match(val => DeleteSelectedOrganizationRegistrations(unitId, ToChangeParametersFromRegistrationDetails(val)),
                    error => error);

            if(deleteRegistrationsResult.HasValue)
                return deleteRegistrationsResult.Value;*/

            _orgUnitService.Delete(unitId);
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

            return Maybe<OperationError>.None;
            /*var parametersList = parameters.ToList();
            return _organizationRightsService.TransferUnitRightsByIds(targetUnitId, GetParametersByType(OrganizationRegistrationType.Roles, parametersList))
                .Match
                (
                    error => error,
                    () => _economyStreamService.TransferRange(targetUnit.Value, GetParametersByType(OrganizationRegistrationType.ExternalPayments, parametersList))
                )
                .Match
                (
                    error => error,
                    () => _economyStreamService.TransferRange(targetUnit.Value, GetParametersByType(OrganizationRegistrationType.InternalPayments, parametersList))
                )
                .Match
                (
                    error => error,
                    () => TransferContractRegistrations(targetUnitId, GetParametersByType(OrganizationRegistrationType.ContractRegistrations, parametersList))
                )
                .Match
                (
                    error => error,
                    () => TransferSystemRelevantRegistrations(unitId, targetUnitId, GetParametersByType(OrganizationRegistrationType.RelevantSystems, parametersList))
                )
                .Match
                (
                    error => error,
                    () => TransferSystemResponsibleRegistrations(targetUnitId, GetParametersByType(OrganizationRegistrationType.ResponsibleSystems, parametersList))
                );*/
        }

        private Maybe<OperationError> TransferContractRegistrations(int targetUnitId, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteResult = _contractService.TransferContractResponsibleUnit(targetUnitId, contractId);
                if (deleteResult.HasValue)
                    return deleteResult.Value;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemovePayments(IEnumerable<PaymentChangeParameters> payments)
        {
            foreach (var payment in payments)
            {
                var removeInternalPaymentsResult = _contractService.RemovePayments(payment.ItContractId, true, payment.InternalPayments);
                if (removeInternalPaymentsResult.HasValue)
                    return removeInternalPaymentsResult.Value;

                var removeExternalPaymentsResult = _contractService.RemovePayments(payment.ItContractId, true, payment.ExternalPayments);
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
                if (deleteResult.Failed)
                    return deleteResult.Error;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemResponsibleRegistrations(int targetUnitId, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteResult = _usageService.TransferResponsibleUsage(targetUnitId, systemId);
                if (deleteResult.Failed)
                    return deleteResult.Error;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> TransferSystemRelevantRegistrations(int unitId, int targetUnitId, IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteResult = _usageService.TransferRelevantUsage(unitId, targetUnitId, systemId);
                if (deleteResult.Failed)
                    return deleteResult.Error;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveSystemResponsibleRegistrations(IEnumerable<int> systemIds)
        {
            foreach (var systemId in systemIds)
            {
                var deleteResult = _usageService.RemoveResponsibleUsage(systemId);
                if (deleteResult.Failed)
                    return deleteResult.Error;
            }

            return Maybe<OperationError>.None;
        }

       /* private static IEnumerable<OrganizationRegistrationChangeParameters> ToChangeParametersFromRegistrationDetails(
            OrganizationRegistrationDetails registrations)
        {
            registrations.ItContractRegistrations.Select(x => )

            return registrations.Select(x => new OrganizationRegistrationChangeParameters(1, OrganizationRegistrationType.ContractRegistrations*//*x.Id, x.Type*//*));
        }*/
    }
}
