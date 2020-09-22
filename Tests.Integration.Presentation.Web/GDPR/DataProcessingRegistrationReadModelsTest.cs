using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.GDPR.Read;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingRegistrationReadModelsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Query_And_Page_ReadModels()
        {
            //Arrange
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var suffix = A<Guid>().ToString("N");
            var name1 = $"1_{suffix}";
            var name2 = $"2_{suffix}";
            var name3 = $"3_{suffix}";

            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name1);
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name2);
            await DataProcessingRegistrationHelper.CreateAsync(organizationId, name3);

            //Act
            var page1 = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

            //Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(name1, page1.First().Name);
            Assert.Equal(name2, page1.Last().Name);

            Assert.Single(page2);
            Assert.Equal(name3, page2.Single().Name);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Content()
        {
            //Arrange
            var name = A<string>();
            var systemName = $"SYSTEM:{name}";
            var refName = $"REF:{name}";
            var refUserAssignedId = $"REF:{name}EXT_ID";
            var refUrl = $"https://www.test-rm{A<uint>()}.dk";
            var refDisp = A<Display>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var agreement = await DataProcessingRegistrationHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var response = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);
            await ReferencesHelper.CreateReferenceAsync(refName, refUserAssignedId, refUrl, refDisp, dto => dto.DataProcessingRegistration_Id = agreement.Id);
            var itSystemDto = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(itSystemDto.Id, organizationId);
            using var assignSystemResponse = await DataProcessingRegistrationHelper.SendAssignSystemRequestAsync(agreement.Id, itSystemDto.Id);

            //Wait for read model to rebuild
            await Task.WhenAll(
                WaitForAsync(() =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<DataProcessingRegistrationRoleAssignmentReadModel, bool>(x =>
                            x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id)));
                }, TimeSpan.FromSeconds(10)),
                WaitForAsync(() =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<DataProcessingRegistrationReadModel, bool>(x =>
                            x.AsQueryable().Any(rm => rm.MainReferenceUrl == refUrl)));
                }, TimeSpan.FromSeconds(10)),
                WaitForAsync(() =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<DataProcessingRegistrationReadModel, bool>(x =>
                            x.AsQueryable().Any(rm => rm.SystemNamesAsCsv.Contains(systemName))));
                }, TimeSpan.FromSeconds(10))
            );

            //Act
            var result = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(result);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(agreement.Id, readModel.SourceEntityId);
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.Name, roleAssignment.UserFullName);
            Assert.Equal(refName, readModel.MainReferenceTitle);
            Assert.Equal(refUrl, readModel.MainReferenceUrl);
            Assert.Equal(refUserAssignedId, readModel.MainReferenceUserAssignedId);
        }

        [Fact]
        [Description("Tests that child entities are removed from the read model when updated from the source model")]
        public async Task ReadModels_Update_When_Child_Entities_Are_Removed()
        {
            //Arrange
            var name = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var agreement = await DataProcessingRegistrationHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingRegistrationHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingRegistrationHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var response1 = await DataProcessingRegistrationHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Wait for read model to rebuild
            await WaitForAsync(() =>
            {
                return Task.FromResult(
                    DatabaseAccess.MapFromEntitySet<DataProcessingRegistrationRoleAssignmentReadModel, bool>(x =>
                        x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id)));
            }, TimeSpan.FromSeconds(10));

            using var response2 = await DataProcessingRegistrationHelper.SendRemoveRoleRequestAsync(agreement.Id, role.Id, user.Id);


            //Wait for read model to rebuild
            await WaitForAsync(() =>
            {
                return Task.FromResult(
                    DatabaseAccess.MapFromEntitySet<DataProcessingRegistrationRoleAssignmentReadModel, bool>(x =>
                        x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id) == false));
            }, TimeSpan.FromSeconds(10));

            //Act
            var result = (await DataProcessingRegistrationHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(result);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(agreement.Id, readModel.SourceEntityId);
            Assert.Empty(readModel.RoleAssignments);
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
