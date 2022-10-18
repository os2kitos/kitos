using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Model.Organizations;
using Core.ApplicationServices.SystemUsage;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.Organizations
{
    public class OrganizationRegistrationService : IOrganizationRegistrationService
    {
        private readonly IEntityIdentityResolver _identityResolver;
        private readonly IOrganizationService _organizationService;
        private readonly IGenericRepository<ItContract> _contractRepository;
        private readonly IGenericRepository<ItSystemUsage> _systemUsageRepository;
        private readonly IOrganizationRightsService _organizationRightsService;
        private readonly IEconomyStreamService _economyStreamService;
        private readonly IItContractService _contractService;
        private readonly IItSystemUsageService _usageService;

        public OrganizationRegistrationService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IGenericRepository<ItContract> contractRepository, 
            IGenericRepository<ItSystemUsage> systemUsageRepository, 
            IOrganizationRightsService organizationRightsService, 
            IEconomyStreamService economyStreamService, 
            IItContractService contractService,
            IItSystemUsageService usageService)
        {
            _organizationService = organizationService;
            _contractRepository = contractRepository;
            _systemUsageRepository = systemUsageRepository;
            _organizationRightsService = organizationRightsService;
            _economyStreamService = economyStreamService;
            _contractService = contractService;
            _usageService = usageService;
            _identityResolver = identityResolver;
        }

        public Result<OrganizationRegistrationsRoot, OperationError> GetOrganizationRegistrations(int unitId)
        {
            var registrationsRoot = new OrganizationRegistrationsRoot();
            var organizationUnitRights = GetApplicableUnitRights(unitId);

            if (organizationUnitRights.Failed)
                return organizationUnitRights.Error;

            registrationsRoot.Roles = organizationUnitRights.Value.ToList();
            GetContractRegistrations(registrationsRoot, unitId);
            GetSystemRegistrations(registrationsRoot, unitId);

            return registrationsRoot;
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations(OrganizationRegistrationsChangeParameters parameters)
        {
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
                    () => RemoveSystemRelevantUnits(parameters.RelevantSystems)
                );
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations()
        {
            throw new NotImplementedException();
        }

        private Result<IEnumerable<OrganizationRegistrationDetails>, OperationError> GetApplicableUnitRights(int unitId)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }

            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            return unit.Match<Result<IEnumerable<OrganizationRegistrationDetails>, OperationError>>(
                val => val.Rights.Select(x => ToDetails(x.Id, x.Role.Name)).ToList(),
                err => err);
        }

        private void GetContractRegistrations(OrganizationRegistrationsRoot root, int unitId)
        {
            var contracts = _contractRepository.AsQueryable().ByOrganizationId(unitId).ToList();
            var externalPayments = new List<OrganizationRegistrationContractPayment>();
            var internalPayments = new List<OrganizationRegistrationContractPayment>();
            var responsibleOrganizationUnits = new List<OrganizationRegistrationDetails>();

            foreach (var itContract in contracts)
            {
                externalPayments.AddRange(GetPayments(itContract, _economyStreamService.GetExternalEconomyStreams(itContract)));
                internalPayments.AddRange(GetPayments(itContract, _economyStreamService.GetInternalEconomyStreams(itContract)));
                GetResponsibleOrganizationUnit(itContract, responsibleOrganizationUnits);
            }

            root.ExternalPayments = externalPayments;
            root.InternalPayments = internalPayments;
            root.ContractRegistrations = responsibleOrganizationUnits;
        }

        private void GetSystemRegistrations(OrganizationRegistrationsRoot root, int unitId)
        {
            var systems = _systemUsageRepository.AsQueryable().ByOrganizationId(unitId).ToList();
            var responsibleSystems = new List<OrganizationRegistrationDetails>();
            var relevantSystems = new List<OrganizationRegistrationDetailsWithObjectData>();

            foreach (var system in systems)
            {
                relevantSystems.AddRange(GetRelevantSystemRegistrations(system));
                GetResponsibleSystemOrganizationUnit(system, responsibleSystems);
            }

            root.RelevantSystemRegistrations = relevantSystems;
            root.ResponsibleSystemRegistrations = responsibleSystems;
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

        private static void GetResponsibleOrganizationUnit(ItContract contract, ICollection<OrganizationRegistrationDetails> responsibleOrgsList)
        {
            if (contract.ResponsibleOrganizationUnit == null)
                return;

            responsibleOrgsList.Add(ToDetails(contract.Id, contract.ResponsibleOrganizationUnit.Name));
        }

        private static void GetResponsibleSystemOrganizationUnit(ItSystemUsage system, ICollection<OrganizationRegistrationDetails> responsibleOrgsList)
        {
            if (system.ResponsibleUsage == null)
                return;

            responsibleOrgsList.Add(ToDetails(system.ResponsibleUsage.OrganizationUnitId, system.ResponsibleUsage.OrganizationUnit.Name));
        }

        private static IEnumerable<OrganizationRegistrationDetailsWithObjectData> GetRelevantSystemRegistrations(ItSystemUsage systemUsage)
        {
            //TODO: What should I use as a name for the system in which the relevant units are located
            return systemUsage.UsedBy.Select(item => ToDetailsWithObjectData(item.OrganizationUnitId, item.OrganizationUnit.Name, systemUsage.Id, systemUsage.LocalCallName)).ToList();
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

        private Maybe<OperationError> RemoveSystemRelevantUnits(IEnumerable<OrganizationRelevantSystem> relevantSystems)
        {
            foreach (var system in relevantSystems)
            {
                var deleteResult = _usageService.RemoveRelevantUnits(system.SystemId, system.RelevantUnitIds);
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
    }
}
