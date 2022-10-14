using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Organizations;
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
        public OrganizationRegistrationService(IEntityIdentityResolver identityResolver, 
            IOrganizationService organizationService,
            IGenericRepository<ItContract> contractRepository, 
            IGenericRepository<ItSystemUsage> systemUsageRepository)
        {
            _organizationService = organizationService;
            _contractRepository = contractRepository;
            _systemUsageRepository = systemUsageRepository;
            _identityResolver = identityResolver;
        }

        public Result<OrganizationRegistrationsRoot, OperationError> GetOrganizationRegistrations(int unitId)
        {
            var registrationsRoot = new OrganizationRegistrationsRoot();
            var organizationUnitRights = GetOrganizationUnitRights(unitId);

            if (organizationUnitRights.Failed)
                return organizationUnitRights.Error;

            registrationsRoot.Roles = organizationUnitRights.Value.ToList();
            GetContractRegistrations(registrationsRoot, unitId);
            GetSystemRegistrations(registrationsRoot, unitId);

            return registrationsRoot;
        }

        public Maybe<OperationError> DeleteSelectedOrganizationRegistrations()
        {
            throw new NotImplementedException();
        }

        public Maybe<OperationError> TransferSelectedOrganizationRegistrations()
        {
            throw new NotImplementedException();
        }

        private Result<IEnumerable<OrganizationRegistrationDetails>, OperationError> GetOrganizationUnitRights(int unitId)
        {
            return GetApplicableUnitRights(unitId)
                .Match<Result<IEnumerable<OrganizationRegistrationDetails>, OperationError>>(
                    val => val.Select(x => ToDetails(x.Id, x.Object.Name)).ToList(),
                    err => err);
        }

        private Result<IEnumerable<OrganizationUnitRight>, OperationError> GetApplicableUnitRights(int unitId)
        {
            var unitUuid = _identityResolver.ResolveUuid<OrganizationUnit>(unitId);
            if (unitUuid.IsNone)
            {
                return new OperationError("Organization id is invalid", OperationFailure.BadInput);
            }

            var unit = _organizationService.GetOrganizationUnit(unitUuid.Value);
            return unit.Match<Result<IEnumerable<OrganizationUnitRight>, OperationError>>(
                val => val.Rights.ToList(),
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
                externalPayments.AddRange(GetPayments(itContract, itContract.ExternEconomyStreams));
                internalPayments.AddRange(GetPayments(itContract, itContract.InternEconomyStreams));
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

        private static void GetResponsibleOrganizationUnit(ItContract contract, List<OrganizationRegistrationDetails> responsibleOrgsList)
        {
            if (contract.ResponsibleOrganizationUnit == null)
                return;

            responsibleOrgsList.Add(ToDetails(contract.ResponsibleOrganizationUnit.Id, contract.ResponsibleOrganizationUnit.Name));
        }

        private static void GetResponsibleSystemOrganizationUnit(ItSystemUsage system, List<OrganizationRegistrationDetails> responsibleOrgsList)
        {
            if (system.ResponsibleUsage == null)
                return;

            responsibleOrgsList.Add(ToDetails(system.ResponsibleUsage.OrganizationUnitId, system.ResponsibleUsage.OrganizationUnit.Name));
        }

        private static IEnumerable<OrganizationRegistrationDetailsWithObjectData> GetRelevantSystemRegistrations(ItSystemUsage systemUsage)
        {
            var result = new List<OrganizationRegistrationDetailsWithObjectData>();
            var index = 1;
            foreach (var item in systemUsage.UsedBy)
            {
                result.Add(ToDetailsWithObjectData(item.OrganizationUnitId, item.OrganizationUnit.Name, systemUsage.Id, systemUsage.LocalCallName));
                index++;
            }

            return result;
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
                Text = $"{economyStream.AuditDate}, {economyStream.AuditStatus}",
                PaymentIndex = index,
                ObjectId = contract.Id,
                ObjectName = contract.Name,
            };
        }
    }
}
