using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Core.ApplicationServices.Model.Organizations;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
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
        public async Task Can_Delete_All_Selected_Registrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organizationId);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            var selectedRegistrations = ToChangeParametersList(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);
            Assert.Empty(registrationsRootAfterDeletion.Payments);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
        }

        [Fact]
        public async Task Can_Delete_Selected_Registrations_One_By_One()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organizationId);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            //----Check UnitRights deletion----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);

            //----Check Internal Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Single(registrationsRootAfterDeletion.Payments);
            var payment = registrationsRootAfterDeletion.Payments.FirstOrDefault();
            Assert.Empty(payment.InternalPayments);

            //----Check External Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Single(registrationsRootAfterDeletion.Payments);
            payment = registrationsRootAfterDeletion.Payments.FirstOrDefault();
            Assert.Empty(payment.ExternalPayments);

            //----Check Contract registrations deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);
            
            //----Check Responsible unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);

            //----Check Relevant unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
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

            //----Org unit rights----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            var registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit1.Id);
            Assert.Empty(registrationsUnit1.OrganizationUnitRights);

            var registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit2.Id);
            AssertRegistrationIsValid(right, registrationsUnit2.OrganizationUnitRights);

            //----First transfer payments and contractRegistration, than assert----
            //----Internal payments----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            //----External payments----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            //----Contract registrations----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit1.Id);
            Assert.Empty(registrationsUnit1.ItContractRegistrations);
            Assert.Empty(registrationsUnit1.Payments);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit2.Id);
            AssertRegistrationIsValid(contract, registrationsUnit2.ItContractRegistrations);

            Assert.Single(registrationsUnit2.Payments);
            var targetPayment = registrationsUnit2.Payments.FirstOrDefault();
            AssertRegistrationIsValid(externalEconomyStream, targetPayment.ExternalPayments);
            AssertRegistrationIsValid(internalEconomyStream, targetPayment.InternalPayments);

            //----Relevant systems----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit1.Id);
            Assert.Empty(registrationsUnit1.RelevantSystems);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit2.Id);
            AssertRegistrationIsValid(usage, registrationsUnit2.RelevantSystems);

            //----Responsible systems----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(unit1.Id, unit2.Id, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit1.Id);
            Assert.Empty(registrationsUnit1.ItContractRegistrations);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit2.Id);
            AssertRegistrationIsValid(usage, registrationsUnit2.ResponsibleSystems);
        }

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

            Assert.Equal(contractId, paymentRoot.ItContract.Id);
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
                    ItContractId = x.ItContract.Id,
                    InternalPayments = x.InternalPayments.Select(x => x.Id),
                    ExternalPayments = x.ExternalPayments.Select(x => x.Id)
                }),
                RelevantSystems = registrations.RelevantSystems.Select(x => x.Id),
                ResponsibleSystems = registrations.ResponsibleSystems.Select(x => x.Id),
            };
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyUnitRegistrations(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO
            {
                OrganizationUnitRights = registrations.OrganizationUnitRights
            };
            return ToChangeParametersList(dto);
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyInternalPayment(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO()
            {
                Payments = registrations.Payments
                    .Select(x =>
                        new PaymentRegistrationDTO
                        {
                            ItContract = x.ItContract,
                            InternalPayments = x.InternalPayments
                        })
            };
            return ToChangeParametersList(dto);
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyExternalPayment(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO()
            {
                Payments = registrations.Payments
                    .Select(x =>
                        new PaymentRegistrationDTO
                        {
                            ItContract = x.ItContract,
                            ExternalPayments = x.ExternalPayments
                        })
            };
            return ToChangeParametersList(dto);
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyContractRegistrations(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO
            {
                ItContractRegistrations = registrations.ItContractRegistrations
            };
            return ToChangeParametersList(dto);
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyResponsibleSystems(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO
            {
                ResponsibleSystems = registrations.ResponsibleSystems
            };
            return ToChangeParametersList(dto);
        }

        private static ChangeOrganizationRegistrationRequest CreateChangeParametersWithOnlyRelevantSystems(
            OrganizationRegistrationDTO registrations)
        {
            var dto = new OrganizationRegistrationDTO
            {
                RelevantSystems = registrations.RelevantSystems
            };
            return ToChangeParametersList(dto);
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
