using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Extensions;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.SystemRelations;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage.V2
{
    public class ItSystemUsageApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task Can_Get_All_ItSystemUsages()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var system3UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token)).ToList();

            //Assert
            Assert.Equal(4, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
            AssertExpectedUsageShallow(system3UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_Organization()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, organization1.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_SystemUuId()
        {
            //Arrange
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, systemUuidFilter: system2.uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_RelationToSystemUuId()
        {
            //Arrange - setup multiple relations across orgs
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var system3UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);
            var system4UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization1.Id);
            var system4UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization2.Id);

            await CreateRelationAsync(system1UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, system4UsageOrg1);
            await CreateRelationAsync(system2UsageOrg2, system4UsageOrg2);
            await CreateRelationAsync(system4UsageOrg2, system3UsageOrg2);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToSystemUuidFilter: relationTargetSystem.uuid)).ToList();

            //Assert
            Assert.Equal(3, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg2, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_RelationToSystemUsageUuId()
        {
            //Arrange - setup multiple relations across orgs
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public);
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);
            var relationTargetSystem = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            var system2UsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization2.Id);
            var relationTargetUsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization1.Id);
            var relationTargetUsageOrg2 = await ItSystemHelper.TakeIntoUseAsync(relationTargetSystem.dbId, organization2.Id);

            await CreateRelationAsync(system1UsageOrg1, relationTargetUsageOrg1);
            await CreateRelationAsync(system2UsageOrg1, relationTargetUsageOrg1);
            await CreateRelationAsync(system2UsageOrg2, relationTargetUsageOrg2);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, relationToSystemUsageUuidFilter: relationTargetUsageOrg1.Uuid)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_All_ItSystemUsages_Filtered_By_SystemNameContent()
        {
            //Arrange
            var content = $"CONTENT_{A<Guid>()}";
            var organization1 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var organization2 = await CreateOrganizationAsync(A<OrganizationTypeKeys>());
            var (user, token) = await CreateApiUser(organization1);
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization1.Id).DisposeAsync();
            await HttpApi.SendAssignRoleToUserAsync(user.Id, OrganizationRole.LocalAdmin, organization2.Id).DisposeAsync();

            var system1 = await CreateSystemAsync(organization1.Id, AccessModifier.Public, $"{content}ONE");
            var system2 = await CreateSystemAsync(organization2.Id, AccessModifier.Public, $"TWO{content}");
            var system3 = await CreateSystemAsync(organization2.Id, AccessModifier.Public);

            var system1UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system1.dbId, organization1.Id);
            var system2UsageOrg1 = await ItSystemHelper.TakeIntoUseAsync(system2.dbId, organization1.Id);
            await ItSystemHelper.TakeIntoUseAsync(system3.dbId, organization2.Id);

            //Act
            var dtos = (await ItSystemUsageV2Helper.GetManyAsync(token, systemNameContentFilter: content)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            AssertExpectedUsageShallow(system1UsageOrg1, dtos);
            AssertExpectedUsageShallow(system2UsageOrg1, dtos);
        }

        [Fact]
        public async Task Can_Get_Specific_ItSystemUsage()
        {
            //Arrange

            //Act

            //Assert TODO: Assert a full dto here to make sure everything is mapped correctly over the wire
        }

        [Fact]
        public async Task Cannot_Get_Unknown_ItSystemUsage()
        {
            //Arrange

            //Act

            //Assert
        }

        [Fact]
        public async Task Cannot_Get_ItSystemUsage_If_NotAllowedTo()
        {
            //Arrange

            //Act

            //Assert
        }

        private string CreateEmail()
        {
            return $"{CreateName()}{DateTime.Now.Ticks}@kitos.dk";
        }

        private static void AssertExpectedUsageShallow(ItSystemUsageDTO expectedContent, IEnumerable<ItSystemUsageResponseDTO> dtos)
        {
            var dto = Assert.Single(dtos, usage => usage.Uuid == expectedContent.Uuid);
            Assert.Equal(expectedContent.Organization.Uuid, dto.OrganizationContext.Uuid);
            Assert.Equal(expectedContent.Organization.Name, dto.OrganizationContext.Name);
            Assert.Equal(expectedContent.Organization.Cvr, dto.OrganizationContext.Cvr);
            Assert.Equal(expectedContent.ItSystem.Uuid, dto.SystemContext.Uuid);
            Assert.Equal(expectedContent.ItSystem.Name, dto.SystemContext.Name);

        }
        private async Task<(Guid uuid, int dbId)> CreateSystemAsync(int organizationId, AccessModifier accessModifier, string name = null)
        {
            var systemName = name ?? CreateName();
            var createdSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, accessModifier);
            var entityUuid = DatabaseAccess.GetEntityUuid<Core.DomainModel.ItSystem.ItSystem>(createdSystem.Id);

            return (entityUuid, createdSystem.Id);
        }

        private async Task<(User user, string token)> CreateApiUser(Organization organization)
        {
            var userAndGetToken = await HttpApi.CreateUserAndGetToken(CreateEmail(), OrganizationRole.User, organization.Id, true, false);
            var user = DatabaseAccess.MapFromEntitySet<User, User>(x => x.AsQueryable().ById(userAndGetToken.userId));
            return (user, userAndGetToken.token);
        }

        private async Task<Organization> CreateOrganizationAsync(OrganizationTypeKeys orgType)
        {
            var organizationName = CreateName();
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                organizationName, string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), orgType, AccessModifier.Public);
            return organization;
        }

        private string CreateName()
        {
            return $"{nameof(ItSystemUsageApiV2Test)}{A<string>()}";
        }

        private static async Task<SystemRelationDTO> CreateRelationAsync(ItSystemUsageDTO fromUsage, ItSystemUsageDTO toUsage)
        {
            return await SystemRelationHelper.PostRelationAsync(new CreateSystemRelationDTO { FromUsageId = fromUsage.Id, ToUsageId = toUsage.Id });
        }
    }
}
