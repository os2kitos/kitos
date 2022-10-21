using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItContract;
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
        private readonly IEconomyStreamService _economyStreamService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;
        private readonly IAuthorizationContext _authorizationContext;
        private readonly IOrgUnitService _orgUnitService;

        public OrganizationRegistrationService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IOrganizationRightsService organizationRightsService, 
            IEconomyStreamService economyStreamService, 
            IItContractService contractService,
            IItSystemUsageService usageService, 
            IAuthorizationContext authorizationContext, 
            IOrgUnitService orgUnitService)
        {
            _organizationService = organizationService;
            _organizationRightsService = organizationRightsService;
            _economyStreamService = economyStreamService;
            _contractService = contractService;
            _usageService = usageService;
            _authorizationContext = authorizationContext;
            _orgUnitService = orgUnitService;
            _identityResolver = identityResolver;
        }

        public Result<IEnumerable<OrganizationRegistrationDetails>, OperationError> GetOrganizationRegistrations(int unitId)
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

            var registrations = new List<OrganizationRegistrationDetails>();
            registrations.AddRange(GetApplicableUnitRights(unit.Value));
            registrations.AddRange(GetContractRegistrations(unitId));
            registrations.AddRange(GetSystemRegistrations(unitId));

            return registrations;
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, IEnumerable<OrganizationRegistrationChangeParameters> parameters)
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

            var parametersList = parameters.ToList();
            return _organizationRightsService.RemoveSelectedUnitRights(GetParametersByType(OrganizationRegistrationType.Roles, parametersList))
                .Match
                (
                    error => error,
                () => _economyStreamService.DeleteRange(GetParametersByType(OrganizationRegistrationType.ExternalPayments, parametersList))
                )
                .Match
                (
                    error => error,
                    () => _economyStreamService.DeleteRange(GetParametersByType(OrganizationRegistrationType.InternalPayments, parametersList))
                )
                .Match
                (
                    error => error,
                    () => RemoveContractRegistrations(GetParametersByType(OrganizationRegistrationType.ContractRegistrations, parametersList))
                )
                .Match
                (
                    error => error,
                    () => RemoveSystemResponsibleRegistrations(GetParametersByType(OrganizationRegistrationType.ResponsibleSystems, parametersList))
                )
                .Match
                (
                    error => error,
                    () => RemoveSystemRelevantUnits(GetParametersByType(OrganizationRegistrationType.RelevantSystems, parametersList), unitId)
                );
        }

        public Maybe<OperationError> DeleteSingleOrganizationRegistration(int unitId, OrganizationRegistrationChangeParameters parameters)
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

            return parameters.Type switch
            {
                OrganizationRegistrationType.Roles => _organizationRightsService.RemoveUnitRole(parameters.Id),
                OrganizationRegistrationType.ExternalPayments or OrganizationRegistrationType.InternalPayments => 
                    _economyStreamService.Delete(parameters.Id),
                OrganizationRegistrationType.ContractRegistrations => _contractService.RemoveContractResponsibleUnit(parameters.Id)
                    .Match(val => Maybe<OperationError>.None, error => error),
                OrganizationRegistrationType.ResponsibleSystems => _usageService.RemoveResponsibleUsage(parameters.Id)
                    .Match(val => Maybe<OperationError>.None, error => error),
                OrganizationRegistrationType.RelevantSystems => _usageService.RemoveRelevantUnit(parameters.Id, unitId)
                    .Match(val => Maybe<OperationError>.None, error => error),
                _ => new OperationError("Incorrect OrganizationRegistrationType", OperationFailure.BadInput)
            };
        }

        public Maybe<OperationError> DeleteUnitWithOrganizationRegistrations(int unitId)
        {
            var deleteRegistrationsResult = GetOrganizationRegistrations(unitId)
                .Match(val => DeleteSelectedOrganizationRegistrations(unitId, ToChangeParametersFromRegistrationDetails(val)),
                    error => error);

            if(deleteRegistrationsResult.HasValue)
                return deleteRegistrationsResult.Value;

            _orgUnitService.Delete(unitId);
            return Maybe<OperationError>.None;
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, IEnumerable<OrganizationRegistrationChangeParameters> parameters)
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

            var parametersList = parameters.ToList();
            return _organizationRightsService.TransferSelectedUnitRights(targetUnitId, GetParametersByType(OrganizationRegistrationType.Roles, parametersList))
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
                );
        }

        private static IEnumerable<OrganizationRegistrationDetails> GetApplicableUnitRights(OrganizationUnit unit)
        { 
            return unit.Rights.Select(x => ToDetails(x.Id, x.Role.Name, OrganizationRegistrationType.Roles)).ToList();
        }

        private IEnumerable<OrganizationRegistrationDetails> GetContractRegistrations(int unitId)
        {
            var contracts = _contractService.GetContractsByResponsibleUnitId(unitId);
            var contractRegistrations = new List<OrganizationRegistrationDetails>();

            foreach (var itContract in contracts)
            {
                contractRegistrations.AddRange(GetPayments(itContract, _economyStreamService.GetExternalEconomyStreamsByUnitId(itContract, unitId), OrganizationRegistrationType.ExternalPayments));
                contractRegistrations.AddRange(GetPayments(itContract, _economyStreamService.GetInternalEconomyStreamsByUnitId(itContract, unitId), OrganizationRegistrationType.InternalPayments));
                contractRegistrations.Add(ToDetails(itContract.Id, itContract.Name, OrganizationRegistrationType.ContractRegistrations));
            }

            return contractRegistrations;
        }

        private IEnumerable<OrganizationRegistrationDetails> GetSystemRegistrations(int unitId)
        {
            var responsibleSystems = _usageService.GetSystemsByResponsibleUnitId(unitId);
            var relevantSystems = _usageService.GetSystemsByRelevantUnitId(unitId);

            //TODO: What to use as a name?
            var result = new List<OrganizationRegistrationDetails>();
            result.AddRange(responsibleSystems.Select(system => ToDetails(system.Id, system.LocalCallName, OrganizationRegistrationType.ResponsibleSystems)).ToList());
            result.AddRange(relevantSystems.Select(system => ToDetails(system.Id, system.LocalCallName, OrganizationRegistrationType.RelevantSystems)).ToList());

            return result;
        }

        private static IEnumerable<OrganizationRegistrationDetails> GetPayments(ItContract contract, IEnumerable<EconomyStream> payments, OrganizationRegistrationType type)
        {
            var result = new List<OrganizationRegistrationDetails>();
            var index = 1;
            foreach (var item in payments)
            {
                var text = $"Acquisition: {item.Acquisition}, Operation: {item.Operation}"; //TODO: How should it look like?
                result.Add(ToDetails(item.Id, text, type, contract.Id, contract.Name, index));
                index++;
            }

            return result;
        }

        private Maybe<OperationError> TransferContractRegistrations(int targetUnitId, IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteResult = _contractService.TransferContractResponsibleUnit(targetUnitId, contractId);
                if (deleteResult.Failed)
                    return deleteResult.Error;
            }

            return Maybe<OperationError>.None;
        }

        private Maybe<OperationError> RemoveContractRegistrations(IEnumerable<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                var deleteResult = _contractService.RemoveContractResponsibleUnit(contractId);
                if (deleteResult.Failed)
                    return deleteResult.Error;
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

        private static IEnumerable<int> GetParametersByType(OrganizationRegistrationType type,
            IEnumerable<OrganizationRegistrationChangeParameters> parameters)
        {
            return parameters.Where(x => x.Type == type).Select(x => x.Id);
        }

        private static OrganizationRegistrationDetails ToDetails(int id, string text, OrganizationRegistrationType type, int? objectId = null, string objectName = "", int? index = null)
        {
            return new OrganizationRegistrationDetails
            {
                Id = id,
                Text = text,
                Type = type,
                PaymentIndex = index,
                ObjectId = objectId,
                ObjectName = objectName
            };
        }

        private static IEnumerable<OrganizationRegistrationChangeParameters> ToChangeParametersFromRegistrationDetails(
            IEnumerable<OrganizationRegistrationDetails> registrations)
        {
            return registrations.Select(x => new OrganizationRegistrationChangeParameters(x.Id, x.Type));
        }
    }
}
