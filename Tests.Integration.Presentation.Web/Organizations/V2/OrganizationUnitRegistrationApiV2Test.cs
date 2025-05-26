using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Tracking;
using Core.DomainModel;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.Internal.Organizations;
using Xunit;
using System.Linq;
using Presentation.Web.Models.API.V2.Internal.Common;
using Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools.External;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationUnitRegistrationApiV2Test : BaseTest
    {
        [Fact]
        public async Task Can_Get_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit) = await SetupRegistrations(organization.Uuid);

            var registrationsRoot = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);

            AssertRegistrationsAreValid(right, contract, externalEconomyStream, internalEconomyStream, usage, registrationsRoot);
        }

        [Fact]
        public async Task Can_Delete_All_Selected_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Uuid);

            var registrations = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);

            var selectedRegistrations = ToChangeParametersList(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
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
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Uuid);

            var registrations = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);

            //----Check UnitRights deletion----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.OrganizationUnitRights);

            //----Check Internal Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Single(registrationsRootAfterDeletion.Payments);
            var payment = registrationsRootAfterDeletion.Payments.FirstOrDefault();
            Assert.Empty(payment.InternalPayments);

            //----Check External Payment deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.Payments);

            //----Check Contract registrationsUnit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.ItContractRegistrations);

            //----Check Responsible unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystems);

            //----Check Relevant unit deletion----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit.Uuid, selectedRegistrations);

            registrationsRootAfterDeletion = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit.Uuid);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystems);
        }

        [Fact]
        public async Task Can_Delete_Unit_With_All_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationUuid = organization.Uuid;
            var (_, _, _, _, _, unit) = await SetupRegistrations(organization.Uuid);

            await OrganizationUnitV2Helper.SendDeleteUnitAsync(organizationUuid, unit.Uuid)
                .WithExpectedResponseCode(HttpStatusCode.OK).DisposeAsync();

            var organizationUnits =
                await OrganizationUnitV2Helper.GetOrganizationUnitsAsync(await GetGlobalToken(), organizationUuid, 0, 100);
            Assert.DoesNotContain(unit.Uuid, organizationUnits.Select(x => x.Uuid));

            using var registrationsResponse = await OrganizationUnitRegistrationV2Helper.SendGetRegistrationsAsync(organizationUuid, unit.Uuid);
            Assert.Equal(HttpStatusCode.NotFound, registrationsResponse.StatusCode);
            var deletionTrackingFound = DatabaseAccess.MapFromEntitySet<LifeCycleTrackingEvent, bool>(all => all.AsQueryable().Any(track => track.EntityUuid == unit.Uuid));
            Assert.True(deletionTrackingFound);
        }

        [Fact]
        public async Task Can_Transfer_Registrations()
        {
            var organization = await CreateOrganizationAsync();
            var organizationId = organization.Uuid;
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit1) = await SetupRegistrations(organization.Uuid);
            var unit2 = await CreateOrganizationUnitAsync(organization.Uuid);
            var unit2Id = DatabaseAccess.GetEntityId<OrganizationUnit>(unit2.Uuid);

            var registrations = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);

            //----Org unit rights----
            var selectedRegistrations = CreateChangeParametersWithOnlyUnitRegistrations(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            var registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.OrganizationUnitRights);

            var registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.NotEmpty(registrationsUnit2.OrganizationUnitRights);

            var rights = DatabaseAccess.MapFromEntitySet<OrganizationUnitRight, List<OrganizationUnitRight>>(x =>
                x.AsQueryable()
                .Where(xc =>
                    xc.RoleId == right.RoleId
                    && xc.ObjectId == unit2Id
                    && xc.UserId == right.UserId)
                .ToList());

            Assert.Single(rights);
            var newRight = rights.FirstOrDefault();
            Assert.NotEqual(right.Id, newRight.Id);

            //----Internal payments----
            selectedRegistrations = CreateChangeParametersWithOnlyInternalPayment(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Single(registrationsUnit1.Payments);
            var payment = registrationsUnit1.Payments.FirstOrDefault();
            Assert.Empty(payment.InternalPayments);

            registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.Single(registrationsUnit2.Payments);
            var targetPayment = registrationsUnit2.Payments.FirstOrDefault();
            AssertRegistrationIsValid(internalEconomyStream, targetPayment.InternalPayments);

            //----External payments----
            selectedRegistrations = CreateChangeParametersWithOnlyExternalPayment(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.Payments);

            registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            Assert.Single(registrationsUnit2.Payments);
            targetPayment = registrationsUnit2.Payments.FirstOrDefault();
            AssertRegistrationIsValid(externalEconomyStream, targetPayment.ExternalPayments);

            //----Contract registrationsUnit----
            selectedRegistrations = CreateChangeParametersWithOnlyContractRegistrations(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.ItContractRegistrations);
            Assert.Empty(registrationsUnit1.Payments);

            registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(contract, registrationsUnit2.ItContractRegistrations);

            //----Responsible systems----
            selectedRegistrations = CreateChangeParametersWithOnlyResponsibleSystems(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.ResponsibleSystems);

            registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(usage, registrationsUnit2.RelevantSystems);
            AssertRegistrationIsValid(usage, registrationsUnit2.ResponsibleSystems);


            //----Relevant systems----
            selectedRegistrations = CreateChangeParametersWithOnlyRelevantSystems(registrations, unit2.Uuid);
            await OrganizationUnitRegistrationV2Helper.DeleteSelectedRegistrationsAsync(organizationId, unit2.Uuid, selectedRegistrations);
            await OrganizationUnitRegistrationV2Helper.TransferRegistrationsAsync(organizationId, unit1.Uuid, selectedRegistrations);

            registrationsUnit1 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit1.Uuid);
            Assert.Empty(registrationsUnit1.RelevantSystems);

            registrationsUnit2 = await OrganizationUnitRegistrationV2Helper.GetRegistrationsAsync(organizationId, unit2.Uuid);
            AssertRegistrationIsValid(usage, registrationsUnit2.RelevantSystems);
        }

        private static void AssertRegistrationsAreValid(OrganizationUnitRight right, ItContract contract,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrganizationRegistrationUnitResponseDTO registrationsUnit)
        {
            Assert.NotNull(registrationsUnit);

            AssertRegistrationIsValid(right, registrationsUnit.OrganizationUnitRights);
            AssertPaymentIsValid(contract.Uuid, externalEconomyStream, internalEconomyStream, registrationsUnit.Payments);
            AssertRegistrationIsValid(contract, registrationsUnit.ItContractRegistrations);
            AssertRegistrationIsValid(usage, registrationsUnit.ResponsibleSystems);
            AssertRegistrationIsValid(usage, registrationsUnit.RelevantSystems);
        }

        private static void AssertPaymentIsValid(Guid contractUuid,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream,
            IEnumerable<PaymentRegistrationResponseDTO> payments)
        {
            var paymentList = payments.ToList();

            Assert.Single(paymentList);
            var paymentRoot = paymentList.FirstOrDefault();

            Assert.Equal(contractUuid, paymentRoot.ItContract.Uuid);
            AssertRegistrationIsValid(externalEconomyStream, paymentRoot.ExternalPayments);
            AssertRegistrationIsValid(internalEconomyStream, paymentRoot.InternalPayments);
        }

        private static void AssertRegistrationIsValid(IHasId expectedObject,
            IEnumerable<NamedEntityV2DTO> registrations)
        {
            var registrationList = registrations.ToList();
            Assert.Single(registrationList);
            Assert.Contains(expectedObject.Id, registrationList.Select(x => x.Id));
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO ToChangeParametersList(OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            return new TransferOrganizationUnitRegistrationV2RequestDTO
            {
                TargetUnitUuid = targetUnitUuid.GetValueOrDefault(),
                ItContractRegistrations = registrationsUnit.ItContractRegistrations.Select(x => x.Id).ToList(),
                OrganizationUnitRights = registrationsUnit.OrganizationUnitRights.Select(x => x.Id).ToList(),
                PaymentRegistrationDetails = registrationsUnit.Payments.Select(x => new ChangePaymentRegistrationV2RequestDTO()
                {
                    ItContractId = x.ItContractId,
                    InternalPayments = x.InternalPayments.Select(payment => payment.Id).ToList(),
                    ExternalPayments = x.ExternalPayments.Select(payment => payment.Id).ToList()
                }).ToList(),
                RelevantSystems = registrationsUnit.RelevantSystems.Select(x => x.Id).ToList(),
                ResponsibleSystems = registrationsUnit.ResponsibleSystems.Select(x => x.Id).ToList(),
            };
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyUnitRegistrations(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO
            {
                OrganizationUnitRights = registrationsUnit.OrganizationUnitRights.Select(x => new NamedEntityWithUserFullNameV2DTO(x.Id, x.Uuid, x.Name, x.UserFullName)).ToList(),
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyInternalPayment(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO
            {
                Payments = registrationsUnit.Payments
                    .Select(x =>
                        new PaymentRegistrationResponseDTO
                        {
                            ItContract = x.ItContract,
                            ItContractId = x.ItContractId,
                            InternalPayments = x.InternalPayments
                        })
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyExternalPayment(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO()
            {
                Payments = registrationsUnit.Payments
                    .Select(x =>
                        new PaymentRegistrationResponseDTO()
                        {
                            ItContract = x.ItContract,
                            ItContractId = x.ItContractId,
                            ExternalPayments = x.ExternalPayments
                        })
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyContractRegistrations(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO
            {
                ItContractRegistrations = registrationsUnit.ItContractRegistrations
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyResponsibleSystems(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO
            {
                ResponsibleSystems = registrationsUnit.ResponsibleSystems
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private static TransferOrganizationUnitRegistrationV2RequestDTO CreateChangeParametersWithOnlyRelevantSystems(
            OrganizationRegistrationUnitResponseDTO registrationsUnit, Guid? targetUnitUuid = null)
        {
            var dto = new OrganizationRegistrationUnitResponseDTO
            {
                RelevantSystems = registrationsUnit.RelevantSystems
            };
            return ToChangeParametersList(dto, targetUnitUuid);
        }

        private async Task<(OrganizationUnitRight right, ItContract contract, EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrganizationUnitResponseDTO unitDto)> SetupRegistrations(Guid organizationUuid)
        {
            var (userUuid, _, _) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, organizationUuid);
            var userId = DatabaseAccess.GetEntityId<User>(userUuid);
            var organizationId = DatabaseAccess.GetEntityId<Organization>(organizationUuid);
            var organizationName = A<string>();

            var unit = await CreateOrganizationUnitAsync(organizationUuid, organizationName);
            var unitId = DatabaseAccess.GetEntityId<OrganizationUnit>(unit.Uuid);

            var newRole = new OrganizationUnitRole
            {
                Name = A<string>()
            };
            AssignOwnership(newRole, userUuid);

            var right = new OrganizationUnitRight
            {
                ObjectId = unitId,
                UserId = userId
            };
            AssignOwnership(right, userUuid);

            var internalEconomyStream = CreateEconomyStream(unit.Uuid, userUuid);
            var externalEconomyStream = CreateEconomyStream(unit.Uuid, userUuid);

            var contract = new ItContract
            {
                OrganizationId = organizationId,
                Name = A<string>(),
                InternEconomyStreams = new List<EconomyStream>() { internalEconomyStream },
                ExternEconomyStreams = new List<EconomyStream>() { externalEconomyStream }
            };
            AssignOwnership(contract, userUuid);

            var itSystemUsageOrgUnitUsage = new ItSystemUsageOrgUnitUsage
            {
                OrganizationUnitId = unitId
            };

            var system = new Core.DomainModel.ItSystem.ItSystem
            {
                Name = A<string>(),
                BelongsToId = organizationId,
                OrganizationId = organizationId,
                AccessModifier = AccessModifier.Local
            };
            AssignOwnership(system, userUuid);

            var usage = new ItSystemUsage
            {
                OrganizationId = organizationId,
                ResponsibleUsage = itSystemUsageOrgUnitUsage,
                UsedBy = new List<ItSystemUsageOrgUnitUsage> { itSystemUsageOrgUnitUsage }
            };
            AssignOwnership(usage, userUuid);

            DatabaseAccess.MutateDatabase(context =>
            {
                var unitEntity = context.OrganizationUnits.FirstOrDefault(u => u.Id == unitId);
                if (unitEntity == null)
                    throw new Exception($"Unit with ID: {unitId} was not found!");

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

        private static EconomyStream CreateEconomyStream(Guid unitUuid, Guid userUuid)
        {
            var economy = new EconomyStream
            {
                OrganizationUnitId = DatabaseAccess.GetEntityId<OrganizationUnit>(unitUuid)
            };
            AssignOwnership(economy, userUuid);

            return economy;
        }

        private static void AssignOwnership(IEntity entity, Guid userUuid)
        {
            entity.ObjectOwnerId = DatabaseAccess.GetEntityId<User>(userUuid);
            entity.LastChangedByUserId = DatabaseAccess.GetEntityId<User>(userUuid);
        }
    }
}
