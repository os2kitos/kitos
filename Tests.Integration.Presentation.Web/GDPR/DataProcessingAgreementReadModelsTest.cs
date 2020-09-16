using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.DomainModel.GDPR.Read;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingAgreementReadModelsTest : WithAutoFixture
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

            await DataProcessingAgreementHelper.CreateAsync(organizationId, name1);
            await DataProcessingAgreementHelper.CreateAsync(organizationId, name2);
            await DataProcessingAgreementHelper.CreateAsync(organizationId, name3);

            //Act
            var page1 = (await DataProcessingAgreementHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await DataProcessingAgreementHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

            //Assert
            Assert.Equal(2, page1.Count);
            Assert.Equal(name1, page1.First().Name);
            Assert.Equal(name2, page1.Last().Name);

            Assert.Equal(1, page2.Count);
            Assert.Equal(name3, page2.Single().Name);
        }

        [Fact]
        public async Task ReadModels_Contain_Correct_Content()
        {
            //Arrange
            var name = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var agreement = await DataProcessingAgreementHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var response = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Wait for read model to rebuild
            await WaitForAsync(() =>
            {
                return Task.FromResult(
                    DatabaseAccess.MapFromEntitySet<DataProcessingAgreementRoleAssignmentReadModel, bool>(x =>
                        x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id)));
            }, TimeSpan.FromSeconds(10));

            //Act
            var result = (await DataProcessingAgreementHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(result);
            Assert.Equal(name, readModel.Name);
            Assert.Equal(agreement.Id, readModel.SourceEntityId);
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.Name, roleAssignment.UserFullName);
        }

        [Fact]
        [Description("Tests that child entities are removed from the read model when updated from the source model")]
        public async Task ReadModels_Update_When_Child_Entities_Are_Removed()
        {
            //Arrange
            var name = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var agreement = await DataProcessingAgreementHelper.CreateAsync(organizationId, name);
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var response1 = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Wait for read model to rebuild
            await WaitForAsync(() =>
            {
                return Task.FromResult(
                    DatabaseAccess.MapFromEntitySet<DataProcessingAgreementRoleAssignmentReadModel, bool>(x =>
                        x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id)));
            }, TimeSpan.FromSeconds(10));

            using var response2 = await DataProcessingAgreementHelper.SendRemoveRoleRequestAsync(agreement.Id, role.Id, user.Id);


            //Wait for read model to rebuild
            await WaitForAsync(() =>
            {
                return Task.FromResult(
                    DatabaseAccess.MapFromEntitySet<DataProcessingAgreementRoleAssignmentReadModel, bool>(x =>
                        x.AsQueryable().Any(rm => rm.Parent.SourceEntityId == agreement.Id) == false));
            }, TimeSpan.FromSeconds(10));

            //Act
            var result = (await DataProcessingAgreementHelper.QueryReadModelByNameContent(organizationId, name, 1, 0)).ToList();

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

            Assert.True(conditionMet,$"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
