﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Presentation.Web.Controllers.API.V1.Mapping;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations
{
    public class OrganizationRegistrationTests: WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_Registrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit) = await SetupRegistrations(organizationId);

            var registrationsRoot = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            AssertRegistrationsAreValid(right, contract, externalEconomyStream, internalEconomyStream, usage, registrationsRoot);
        }

        [Fact]
        public async Task Can_Delete_Selected_Registrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organizationId);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            //----Check UnitRights deletion----
            var roleRegistrations = new OrganizationRegistrationDTO()
            {
                OrganizationUnitRights = registrations.OrganizationUnitRights
            };
            var selectedRoleRegistrations = ToChangeParametersList(roleRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRoleRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);

            //----Check Internal Payment deletion----
            var internalPaymentRegistrations = new OrganizationRegistrationDTO()
            {
                Payments = registrations.Payments
                    .Select(x => 
                        new PaymentRegistrationDTO
                        {
                            ContractId = x.ContractId,
                            InternalPayments = x.InternalPayments
                        })
            };
            var selectedRegistrations = ToChangeParametersList(internalPaymentRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.Payments.Select(x => x.InternalPayments));

            //----Check External Payment deletion----
            var externalPaymentRegistrations = new OrganizationRegistrationDTO
            {
                Payments = registrations.Payments
                    .Select(x =>
                        new PaymentRegistrationDTO
                        {
                            ContractId = x.ContractId,
                            ExternalPayments = x.ExternalPayments
                        })
            };
            selectedRegistrations = ToChangeParametersList(externalPaymentRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.Payments.Select(x => x.ExternalPayments));

            //----Check Contract registrations deletion----
            var contractRegistrations = new OrganizationRegistrationDTO
            {
                ItContractRegistrations = registrations.ItContractRegistrations
            };
            selectedRegistrations = ToChangeParametersList(contractRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);
            
            //----Check Responsible unit deletion----
            var responsibleRegistrations = new OrganizationRegistrationDTO
            {
                ResponsibleSystems = registrations.ResponsibleSystems
            };
            selectedRegistrations = ToChangeParametersList(responsibleRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);

            //----Check Relevant unit deletion----
            var relevantRegistrations = new OrganizationRegistrationDTO
            {
                RelevantSystems = registrations.RelevantSystems
            };
            selectedRegistrations = ToChangeParametersList(relevantRegistrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
        }

        /*[Fact]
        public async Task Can_Delete_Single_Registration()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organizationId);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.Roles, registrations);
            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.InternalPayments, registrations);
            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.ExternalPayments, registrations);
            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.ContractRegistrations, registrations);
            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.ResponsibleSystems, registrations);
            await CheckCanDeleteSingleByType(unit.Id, OrganizationRegistrationType.RelevantSystems, registrations);
        }

        [Fact]
        public async Task Can_Delete_Unit_With_All_Registrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organizationId);

            await OrganizationRegistrationHelper.DeleteUnitWithRegistrationsAsync(unit.Id);

            var rootOrganizationUnit = await OrganizationUnitHelper.GetOrganizationUnitsAsync(organizationId);
            Assert.DoesNotContain(unit.Id, rootOrganizationUnit.Children.Select(x => x.Id));

            using var registrationsResponse = await OrganizationRegistrationHelper.SendGetRegistrationsAsync(unit.Id);
            Assert.Equal(HttpStatusCode.BadRequest, registrationsResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Transfer_Registrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit1) = await SetupRegistrations(organizationId);
            var unit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organizationId, A<string>());

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit1.Id);

            await CheckCanTransferByType(unit1.Id, unit2.Id, right, OrganizationRegistrationType.Roles, registrations);
            await CheckCanTransferByType(unit1.Id, unit2.Id, contract, OrganizationRegistrationType.ContractRegistrations, registrations);
            await CheckCanTransferByType(unit1.Id, unit2.Id, internalEconomyStream, OrganizationRegistrationType.InternalPayments, registrations);
            await CheckCanTransferByType(unit1.Id, unit2.Id, externalEconomyStream, OrganizationRegistrationType.ExternalPayments, registrations);
            await CheckCanTransferByType(unit1.Id, unit2.Id, usage, OrganizationRegistrationType.RelevantSystems, registrations);
            await CheckCanTransferByType(unit1.Id, unit2.Id, usage, OrganizationRegistrationType.ResponsibleSystems, registrations);
        }*/

        /*private static async Task CheckCanDeleteByType(int unitId, OrganizationRegistrationType type, IEnumerable<OrganizationRegistrationDetails> registrations)
        {
            var selectedRegistrations = ToChangeParametersList(type, registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unitId, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unitId);
            Assert.Empty(registrationsRootAfterDeletion.Where(x => x.Type == type));
        }*/
        /*
        private static async Task CheckCanDeleteSingleByType(int unitId, OrganizationRegistrationType type, IEnumerable<OrganizationRegistrationDetails> registration)
        {
            var registrationsByType = registration.Where(x => x.Type == type).ToList();
            Assert.Single(registrationsByType);

            var singleRegistration = registrationsByType.FirstOrDefault();
            Assert.NotNull(singleRegistration);

            var selectedRegistrations = ToChangeParameters(singleRegistration);
            await OrganizationRegistrationHelper.DeleteSingleRegistrationAsync(unitId, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unitId);
            Assert.Empty(registrationsRootAfterDeletion.Where(x => x.Type == type));
        }

        private static async Task CheckCanTransferByType(int unitId, int targetUnitId, IHasId expectedObject, OrganizationRegistrationType type, IEnumerable<OrganizationRegistrationDetails> registrations)
        {
            var selectedRegistrations = ToChangeParametersList(type, registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unitId, targetUnitId, selectedRegistrations);

            var registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unitId);
            var res = registrationsUnit1.Where(x => x.Type == type).ToList();
            Assert.Empty(res);

            var registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(targetUnitId);
            AssertRegistrationIsValid(type, expectedObject, registrationsUnit2);
        }*/

        private static void AssertRegistrationsAreValid(OrganizationUnitRight right, ItContract contract,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrganizationRegistrationDTO registrations)
        {
            Assert.NotNull(registrations);

            AssertRegistrationIsValid(right, registrations.OrganizationUnitRights);
            AssertPaymentIsValid(contract.Id, externalEconomyStream, internalEconomyStream, registrations.Payments);
            AssertRegistrationIsValid(contract, registrations.ItContractRegistrations);
            AssertRegistrationIsValid(usage, registrations.ResponsibleSystems);
            AssertRegistrationIsValid(usage, registrations.RelevantSystems);
        }

        private static void AssertPaymentIsValid(int contractId,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream,
            IEnumerable<PaymentRegistrationDTO> payments)
        {
            var paymentList = payments.ToList();

            Assert.Single(paymentList);
            var paymentRoot = paymentList.FirstOrDefault();

            Assert.Equal(contractId, paymentRoot.ContractId);
            AssertRegistrationIsValid(externalEconomyStream, paymentRoot.ExternalPayments);
            AssertRegistrationIsValid(internalEconomyStream, paymentRoot.InternalPayments);
        }

        private static void AssertRegistrationIsValid(IHasId expectedObject,
            IEnumerable<NamedEntityDTO> registrations)
        {
            var registrationList = registrations.ToList();
            Assert.Single(registrationList);
            Assert.Contains(expectedObject.Id, registrationList.Select(x => x.Id));
        }


        private static EconomyStream CreateEconomyStream(int unitId)
        {
            var economy = new EconomyStream
            {
                OrganizationUnitId = unitId
            };
            AssignOwnership(economy);

            return economy;
        }

        private static void AssignOwnership(IEntity entity)
        {
            entity.ObjectOwnerId = TestEnvironment.DefaultUserId;
            entity.LastChangedByUserId = TestEnvironment.DefaultUserId;
        }

        private static ChangeOrganizationRegistrationRequest ToChangeParametersList(OrganizationRegistrationDTO registrations)
        {
            return new ChangeOrganizationRegistrationRequest()
            {
                ItContractRegistrations = registrations.ItContractRegistrations.Select(x => x.Id),
                OrganizationUnitRights = registrations.OrganizationUnitRights.Select(x => x.Id),
                PaymentRegistrationDetails = registrations.Payments.Select(x => new ChangePaymentRegistraitonRequest
                {
                    ItContractId = x.ContractId,
                    InternalPayments = x.InternalPayments.Select(x => x.Id),
                    ExternalPayments = x.ExternalPayments.Select(x => x.Id)
                }),
                RelevantSystems = registrations.RelevantSystems.Select(x => x.Id),
                ResponsibleSystems = registrations.ResponsibleSystems.Select(x => x.Id),
            };
        }

        private async Task<(OrganizationUnitRight right, ItContract contract, EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrgUnitDTO unitDto)> SetupRegistrations(int organizationId)
        {
            var organizationName = A<string>();

            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organizationId, organizationName);

            var newRole = new OrganizationUnitRole
            {
                Name = A<string>()
            };
            AssignOwnership(newRole);

            var right = new OrganizationUnitRight
            {
                ObjectId = unit.Id,
                UserId = TestEnvironment.DefaultUserId
            };
            AssignOwnership(right);

            var internalEconomyStream = CreateEconomyStream(unit.Id);
            var externalEconomyStream = CreateEconomyStream(unit.Id);

            var contract = new ItContract
            {
                OrganizationId = organizationId,
                Name = A<string>(),
                InternEconomyStreams = new List<EconomyStream>() { internalEconomyStream },
                ExternEconomyStreams = new List<EconomyStream>() { externalEconomyStream }
            };
            AssignOwnership(contract);

            var itSystemUsageOrgUnitUsage = new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnitId = unit.Id
            };

            var system = new Core.DomainModel.ItSystem.ItSystem
            {
                Name = A<string>(),
                BelongsToId = organizationId,
                OrganizationId = organizationId,
                AccessModifier = AccessModifier.Local
            };
            AssignOwnership(system);

            var usage = new ItSystemUsage
            {
                OrganizationId = organizationId,
                ResponsibleUsage = itSystemUsageOrgUnitUsage,
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { itSystemUsageOrgUnitUsage }
            };
            AssignOwnership(usage);

            DatabaseAccess.MutateDatabase(context =>
            {
                var unitEntity = context.OrganizationUnits.FirstOrDefault(u => u.Id == unit.Id);
                if (unitEntity == null)
                    throw new Exception($"Unit with ID: {unit.Id} not found!");

                context.OrganizationUnitRoles.Add(newRole);
                right.RoleId = newRole.Id;
                context.OrganizationUnitRights.Add(right);

                unitEntity.Rights = new List<OrganizationUnitRight> { right };
                contract.ResponsibleOrganizationUnit = unitEntity;
                context.ItContracts.Add(contract);

                context.ItSystems.Add(system);
                usage.ItSystemId = system.Id;
                context.ItSystemUsages.Add(usage);

                context.SaveChanges();
            });

            return (right, contract, externalEconomyStream, internalEconomyStream, usage, unit);
        }
    }
}
