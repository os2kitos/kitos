using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class OrganizationUnitTests: WithAutoFixture
    {
        [Fact]
        public async Task GlobalAdmin_Has_All_AccessRights()
        {
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(email, OrganizationRole.GlobalAdmin, organization.Id);
            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, A<string>());

            var accessRights = await OrganizationUnitHelper.GetUnitAccessRights(organization.Uuid, unit.Uuid, cookie);

            Assert.True(accessRights.CanBeRead);
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanNameBeModified);
            Assert.True(accessRights.CanBeRearranged);
            Assert.True(accessRights.CanBeDeleted);
        }

        [Fact]
        public async Task LocalAdmin_From_Organization_Has_All_AccessRights()
        {
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(email, OrganizationRole.LocalAdmin, organization.Id);
            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, A<string>());

            var accessRights = await OrganizationUnitHelper.GetUnitAccessRights(organization.Uuid, unit.Uuid, cookie);

            Assert.True(accessRights.CanBeRead);
            Assert.True(accessRights.CanBeModified);
            Assert.True(accessRights.CanNameBeModified);
            Assert.True(accessRights.CanBeRearranged);
            Assert.True(accessRights.CanBeDeleted);
        }

        [Fact]
        public async Task User_From_Organization_Has_Only_Read_Access()
        {
            var organization = await CreateOrganizationAsync();
            var email = CreateEmail();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(email, OrganizationRole.User, organization.Id);
            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, A<string>());

            var accessRights = await OrganizationUnitHelper.GetUnitAccessRights(organization.Uuid, unit.Uuid, cookie);

            Assert.True(accessRights.CanBeRead);
            Assert.False(accessRights.CanBeModified);
            Assert.False(accessRights.CanNameBeModified);
            Assert.False(accessRights.CanBeRearranged);
            Assert.False(accessRights.CanBeDeleted);
        }

        [Fact]
        public async Task Can_Get_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit) = await SetupRegistrations(organization.Id);

            var registrationsRoot = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);

            AssertRegistrationsAreValid(right, contract, externalEconomyStream, internalEconomyStream, usage, registrationsRoot);
        }

        [Fact]
        public async Task Can_Delete_All_Selected_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Id);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);

            var selectedRegistrations = ToChangeParametersList(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);
            Assert.Empty(registrationsRootAfterDeletion.Payments);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
        }

        [Fact]
        public async Task Can_Delete_Selected_Registrations_One_By_One()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Id);

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);

            //----Check UnitRights deletion----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);

            //----Check Internal Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Single(registrationsRootAfterDeletion.Payments);
            var payment = registrationsRootAfterDeletion.Payments.FirstOrDefault();
            Assert.Empty(payment.InternalPayments);

            //----Check External Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.Payments);

            //----Check Contract registrationsUnit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);
            
            //----Check Responsible unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);

            //----Check Relevant unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
        }

        [Fact]
        public async Task Can_Delete_Unit_With_All_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Id);

            await OrganizationRegistrationHelper.DeleteUnitWithRegistrationsAsync(organizationId, unit.Uuid);

            var rootOrganizationUnit = await OrganizationUnitHelper.GetOrganizationUnitsAsync(organization.Id);
            Assert.DoesNotContain(unit.Id, rootOrganizationUnit.Children.Select(x => x.Id));

            using var registrationsResponse = await OrganizationRegistrationHelper.SendGetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, registrationsResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Transfer_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit1) = await SetupRegistrations(organization.Id);
            var unit2 = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organization.Id, A<string>());

            var registrations = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);

            //----Org unit rights----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            var registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.OrganizationUnitRights);

            var registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.NotEmpty(registrationsUnit2.OrganizationUnitRights);
            
            var rights = DatabaseAccess.MapFromEntitySet<OrganizationUnitRight, List<OrganizationUnitRight>>(x => 
                x.AsQueryable()
                .Where(xc =>
                    xc.RoleId == right.RoleId 
                    && xc.ObjectId == unit2.Id 
                    && xc.UserId == right.UserId)
                .ToList());

            Assert.Single(rights);
            var newRight = rights.FirstOrDefault();
            Assert.NotEqual(right.Id, newRight.Id);
            
            //----Internal payments----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Single(registrationsUnit1.Payments);
            var payment = registrationsUnit1.Payments.FirstOrDefault();
            Assert.Empty(payment.InternalPayments);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.Single(registrationsUnit2.Payments);
            var targetPayment = registrationsUnit2.Payments.FirstOrDefault();
            AssertRegistrationIsValid(internalEconomyStream, targetPayment.InternalPayments);

            //----External payments----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.Payments);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.Single(registrationsUnit2.Payments);
            targetPayment = registrationsUnit2.Payments.FirstOrDefault();
            AssertRegistrationIsValid(externalEconomyStream, targetPayment.ExternalPayments);

            //----Contract registrationsUnit----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.ItContractRegistrations);
            Assert.Empty(registrationsUnit1.Payments);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(contract, registrationsUnit2.ItContractRegistrations);

            //----Responsible systems----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.ResponsibleSystems);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(usage, registrationsUnit2.RelevantSystems);
            AssertRegistrationIsValid(usage, registrationsUnit2.ResponsibleSystems);


            //----Relevant systems----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations, unit2.Uuid);
            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(organizationId, unit2.Uuid, selectedRegistrations);
            await OrganizationRegistrationHelper.TransferRegistrationsAsync(organizationId, unit1.Uuid, unit2.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.RelevantSystems);

            registrationsUnit2 = await OrganizationRegistrationHelper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(usage, registrationsUnit2.RelevantSystems);
        }

        private static void AssertRegistrationsAreValid(OrganizationUnitRight right, ItContract contract,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrganizationRegistrationUnitDTO registrationsUnit)
        {
            Assert.NotNull(registrationsUnit);

            AssertRegistrationIsValid(right, registrationsUnit.OrganizationUnitRights);
            AssertPaymentIsValid(contract.Id, externalEconomyStream, internalEconomyStream, registrationsUnit.Payments);
            AssertRegistrationIsValid(contract, registrationsUnit.ItContractRegistrations);
            AssertRegistrationIsValid(usage, registrationsUnit.ResponsibleSystems);
            AssertRegistrationIsValid(usage, registrationsUnit.RelevantSystems);
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

        private static TransferOrganizationUnitRegistrationRequestDTO ToChangeParametersList(OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            return new TransferOrganizationUnitRegistrationRequestDTO()
            {
                TargetUnitUuid = targetUnitUuid.GetValueOrDefault(),
                ItContractRegistrations = registrationsUnit.ItContractRegistrations.Select(x => x.Id).ToList(),
                OrganizationUnitRights = registrationsUnit.OrganizationUnitRights.Select(x => x.Id).ToList(),
                PaymentRegistrationDetails = registrationsUnit.Payments.Select(x => new ChangePaymentRegistrationRequestDTO
                {
                    ItContractId = x.ItContract.Id,
                    InternalPayments = x.InternalPayments.Select(x => x.Id).ToList(),
                    ExternalPayments = x.ExternalPayments.Select(x => x.Id).ToList()
                }).ToList(),
                RelevantSystems = registrationsUnit.RelevantSystems.Select(x => x.Id).ToList(),
                ResponsibleSystems = registrationsUnit.ResponsibleSystems.Select(x => x.Id).ToList(),
            };
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyUnitRegistrations(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO
            {
                OrganizationUnitRights = registrationsUnit.OrganizationUnitRights
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyInternalPayment(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO
            {
                Payments = registrationsUnit.Payments
                    .Select(x =>
                        new PaymentRegistrationDTO
                        {
                            ItContract = x.ItContract,
                            InternalPayments = x.InternalPayments
                        })
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyExternalPayment(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO()
            {
                Payments = registrationsUnit.Payments
                    .Select(x =>
                        new PaymentRegistrationDTO
                        {
                            ItContract = x.ItContract,
                            ExternalPayments = x.ExternalPayments
                        })
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyContractRegistrations(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO
            {
                ItContractRegistrations = registrationsUnit.ItContractRegistrations
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyResponsibleSystems(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO
            {
                ResponsibleSystems = registrationsUnit.ResponsibleSystems
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationRequestDTO CreateChangeParametersWithOnlyRelevantSystems(
            OrganizationRegistrationUnitDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitDTO
            {
                RelevantSystems = registrationsUnit.RelevantSystems
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private async Task<OrganizationDTO> CreateOrganizationAsync()
        {
            var organizationName = A<string>();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, organizationName, "13370000", OrganizationTypeKeys.Kommune, AccessModifier.Public);
            return organization;
        }

        private string CreateEmail()
        {
            return $"{nameof(OrganizationUnitTests)}{A<string>()}@test.dk";
        }

        private async Task<(OrganizationUnitRight right, ItContract contract, EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrgUnitDTO unitDto)> SetupRegistrations(int organizationId)
        {
            var (userId, _, _) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, organizationId);
            var organizationName = A<string>();

            var unit = await OrganizationHelper.CreateOrganizationUnitRequestAsync(organizationId, organizationName);

            var newRole = new OrganizationUnitRole
            {
                Name = A<string>()
            };
            AssignOwnership(newRole, userId);

            var right = new OrganizationUnitRight
            {
                ObjectId = unit.Id,
                UserId = userId
            };
            AssignOwnership(right, userId);

            var internalEconomyStream = CreateEconomyStream(unit.Id, userId);
            var externalEconomyStream = CreateEconomyStream(unit.Id, userId);

            var contract = new ItContract
            {
                OrganizationId = organizationId,
                Name = A<string>(),
                InternEconomyStreams = new List<EconomyStream>() { internalEconomyStream },
                ExternEconomyStreams = new List<EconomyStream>() { externalEconomyStream }
            };
            AssignOwnership(contract, userId);

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
            AssignOwnership(system, userId);

            var usage = new ItSystemUsage
            {
                OrganizationId = organizationId,
                ResponsibleUsage = itSystemUsageOrgUnitUsage,
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { itSystemUsageOrgUnitUsage }
            };
            AssignOwnership(usage, userId);

            DatabaseAccess.MutateDatabase(context =>
            {
                var unitEntity = context.OrganizationUnits.FirstOrDefault(u => u.Id == unit.Id);
                if (unitEntity == null)
                    throw new Exception($"Unit with ID: {unit.Id} was not found!");

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

        private static EconomyStream CreateEconomyStream(int unitId, int userId)
        {
            var economy = new EconomyStream
            {
                OrganizationUnitId = unitId
            };
            AssignOwnership(economy, userId);

            return economy;
        }

        private static void AssignOwnership(IEntity entity, int userId)
        {
            entity.ObjectOwnerId = userId;
            entity.LastChangedByUserId = userId;
        }
    }
}
