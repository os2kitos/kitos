using System;
using System.Collections.Generic;
using System.Linq;
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
            var (right, contract, externalEconomyStream, internalEconomyStream, usage, unit) = await SetupRegistrations();

            var registrationsRoot = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            AssertRegistrationsAreValid(right, contract, externalEconomyStream, internalEconomyStream, usage, registrationsRoot);
        }

        [Fact]
        public async Task Can_Delete_Selected_Registrations()
        {
            var (_, _, _, _, _, unit) = await SetupRegistrations();

            var registrationsRoot = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            var selectedRegistrations = new RemoveOrganizationRegistrationsRequest
            {
                Roles = registrationsRoot.Roles.Select(x => x.Id).ToList(),
                ContractRegistrations = registrationsRoot.ContractRegistrations.Select(x => x.Id).ToList(),
                ExternalPayments = registrationsRoot.ExternalPayments.Select(x => x.Id).ToList(),
                InternalPayments = registrationsRoot.InternalPayments.Select(x => x.Id).ToList(),
                RelevantSystems = registrationsRoot.RelevantSystemRegistrations.Select(x => x.Id).ToList(),
                ResponsibleSystems = registrationsRoot.ResponsibleSystemRegistrations.Select(x => x.Id).ToList()
            };

            await OrganizationRegistrationHelper.DeleteSelectedRegistrationsAsync(unit.Id, selectedRegistrations);

            var registrationsRootAfterDeletion = await OrganizationRegistrationHelper.GetRegistrationsAsync(unit.Id);

            Assert.Empty(registrationsRootAfterDeletion.Roles);
            Assert.Empty(registrationsRootAfterDeletion.ContractRegistrations);
            Assert.Empty(registrationsRootAfterDeletion.ExternalPayments);
            Assert.Empty(registrationsRootAfterDeletion.InternalPayments);
            Assert.Empty(registrationsRootAfterDeletion.RelevantSystemRegistrations);
            Assert.Empty(registrationsRootAfterDeletion.ResponsibleSystemRegistrations);
        }

        private async Task<(OrganizationUnitRight right, ItContract contract, EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrgUnitDTO unitDto)> SetupRegistrations()
        {
            var organizationId = TestEnvironment.DefaultOrganizationId;
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

        private static void AssertRegistrationsAreValid(OrganizationUnitRight right, ItContract contract,
            EconomyStream externalEconomyStream, EconomyStream internalEconomyStream, ItSystemUsage usage, OrganizationRegistrationsRoot registrationsRoot)
        {
            Assert.NotNull(registrationsRoot);

            Assert.Single(registrationsRoot.Roles);
            Assert.Single(registrationsRoot.ContractRegistrations);
            Assert.Single(registrationsRoot.InternalPayments);
            Assert.Single(registrationsRoot.ExternalPayments);
            Assert.Single(registrationsRoot.RelevantSystemRegistrations);
            Assert.Single(registrationsRoot.ResponsibleSystemRegistrations);

            Assert.Contains(right.Id, registrationsRoot.Roles.Select(x => x.Id));
            Assert.Contains(contract.Id, registrationsRoot.ContractRegistrations.Select(x => x.Id));
            Assert.Contains(internalEconomyStream.Id, registrationsRoot.InternalPayments.Select(x => x.Id));
            Assert.Contains(externalEconomyStream.Id, registrationsRoot.ExternalPayments.Select(x => x.Id));
            Assert.Contains(usage.Id, registrationsRoot.RelevantSystemRegistrations.Select(x => x.Id));
            Assert.Contains(usage.Id, registrationsRoot.ResponsibleSystemRegistrations.Select(x => x.Id));
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
    }
}
