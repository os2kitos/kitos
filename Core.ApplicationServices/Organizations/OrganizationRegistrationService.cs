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

        public Result<OrganizationRegistrationsRoot, OperationError> GetOrganizationRegistrations(int unitId)
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

            var registrationsRoot = new OrganizationRegistrationsRoot();
            
            GetApplicableUnitRights(registrationsRoot, unit.Value);
            GetContractRegistrations(registrationsRoot, unitId);
            GetSystemRegistrations(registrationsRoot, unitId);

            return registrationsRoot;
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(int unitId, OrganizationRegistrationsChangeParameters parameters)
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

            return _organizationRightsService.RemoveSelectedUnitRights(parameters.RoleIds)
                .Match
                (
                    error => error,
                () => _economyStreamService.DeleteRange(parameters.ExternalPaymentIds)
                )
                .Match
                (
                    error => error,
                    () => _economyStreamService.DeleteRange(parameters.InternalPaymentIds)
                )
                .Match
                (
                    error => error,
                    () => RemoveContractRegistrations(parameters.ContractWithRegistrationIds)
                )
                .Match
                (
                    error => error,
                    () => RemoveSystemResponsibleRegistrations(parameters.ResponsibleSystemIds)
                )
                .Match
                (
                    error => error,
                    () => RemoveSystemRelevantUnits(parameters.RelevantSystems, unitId)
                );
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

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations(int unitId, int targetUnitId, OrganizationRegistrationsChangeParameters parameters)
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

            return _organizationRightsService.TransferSelectedUnitRights(targetUnitId, parameters.RoleIds)
                .Match
                (
                    error => error,
                    () => _economyStreamService.TransferRange(targetUnit.Value, parameters.ExternalPaymentIds)
                )
                .Match
                (
                    error => error,
                    () => _economyStreamService.TransferRange(targetUnit.Value, parameters.InternalPaymentIds)
                )
                .Match
                (
                    error => error,
                    () => TransferContractRegistrations(targetUnitId, parameters.ContractWithRegistrationIds)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemRelevantRegistrations(unitId, targetUnitId, parameters.ResponsibleSystemIds)
                )
                .Match
                (
                    error => error,
                    () => TransferSystemResponsibleRegistrations(targetUnitId, parameters.RelevantSystems)
                );
        }

        private static void GetApplicableUnitRights(OrganizationRegistrationsRoot root, OrganizationUnit unit)
        { 
            root.Roles = unit.Rights.Select(x => ToDetails(x.Id, x.Role.Name)).ToList();
        }

        private void GetContractRegistrations(OrganizationRegistrationsRoot root, int unitId)
        {
            var contracts = _contractService.GetContractsByResponsibleUnitId(unitId);
            var externalPayments = new List<OrganizationRegistrationContractPayment>();
            var internalPayments = new List<OrganizationRegistrationContractPayment>();
            var responsibleOrganizationUnits = new List<OrganizationRegistrationDetails>();

            foreach (var itContract in contracts)
            {
                externalPayments.AddRange(GetPayments(itContract, _economyStreamService.GetExternalEconomyStreamsByUnitId(itContract, unitId)));
                internalPayments.AddRange(GetPayments(itContract, _economyStreamService.GetInternalEconomyStreamsByUnitId(itContract, unitId)));
                responsibleOrganizationUnits.Add(ToDetails(itContract.Id, itContract.Name));
            }

            root.ExternalPayments = externalPayments;
            root.InternalPayments = internalPayments;
            root.ContractRegistrations = responsibleOrganizationUnits;
        }

        private void GetSystemRegistrations(OrganizationRegistrationsRoot root, int unitId)
        {
            var responsibleSystems = _usageService.GetSystemsByResponsibleUnitId(unitId);
            var relevantSystems = _usageService.GetSystemsByRelevantUnitId(unitId);

            //TODO: What to use as a name?
            var responsibleSystemRegistrations = responsibleSystems.Select(system => ToDetails(system.Id, system.LocalCallName)).ToList();
            var relevantSystemRegistrations = relevantSystems.Select(system => ToDetails(system.Id, system.LocalCallName)).ToList();

            root.RelevantSystemRegistrations = relevantSystemRegistrations;
            root.ResponsibleSystemRegistrations = responsibleSystemRegistrations;
        }

        private static IEnumerable<OrganizationRegistrationContractPayment> GetPayments(ItContract contract, IEnumerable<EconomyStream> payments)
        {
            var result = new List<OrganizationRegistrationContractPayment>();
            var index = 1;
            foreach (var item in payments)
            {
                result.Add(ToContractPaymentFromEconomyStream(index, item, contract));
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

        private static OrganizationRegistrationDetails ToDetails(int id, string text)
        {
            return new OrganizationRegistrationDetails
            {
                Id = id,
                Text = text
            };
        }

        private static OrganizationRegistrationDetailsWithObjectData ToDetailsWithObjectData(int id, string text, int objectId, string objectName)
        {
            return new OrganizationRegistrationDetailsWithObjectData
            {
                Id = id,
                Text = text,
                ObjectId = objectId,
                ObjectName = objectName,
            };
        }

        private static OrganizationRegistrationContractPayment ToContractPaymentFromEconomyStream(int index, EconomyStream economyStream, ItContract contract)
        {
            return new OrganizationRegistrationContractPayment
            {
                Id = economyStream.Id,
                Text = $"Acquisition: {economyStream.Acquisition}, Operation: {economyStream.Operation}", //TODO: How should it look like?
                PaymentIndex = index,
                ObjectId = contract.Id,
                ObjectName = contract.Name,
            };
        }

        private static OrganizationRegistrationsChangeParameters ToChangeParametersFromRegistrationDetails(
            OrganizationRegistrationsRoot root)
        {
            return new OrganizationRegistrationsChangeParameters()
            {
                ContractWithRegistrationIds = root.ContractRegistrations.Select(x => x.Id).ToList(),
                ExternalPaymentIds = root.ExternalPayments.Select(x => x.Id).ToList(),
                InternalPaymentIds = root.InternalPayments.Select(x => x.Id).ToList(),
                RelevantSystems = root.RelevantSystemRegistrations.Select(x => x.Id).ToList(),
                ResponsibleSystemIds = root.ResponsibleSystemRegistrations.Select(x => x.Id).ToList(),
                RoleIds = root.Roles.Select(x => x.Id).ToList()
            };
        }
    }
}
