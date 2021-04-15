using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.SystemUsage
{
    public class ItSystemUsageOverviewReadModelsTest : WithAutoFixture
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

            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name1, organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name2, organizationId, AccessModifier.Public);
            var system3 = await ItSystemHelper.CreateItSystemInOrganizationAsync(name3, organizationId, AccessModifier.Public);

            await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system3.Id, organizationId);


            //Act
            var page1 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 0)).ToList();
            var page2 = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, suffix, 2, 2)).ToList();

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
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var organizationName = TestEnvironment.DefaultOrganizationName;

            var systemName = A<string>();
            var systemDisabled = A<bool>();

            var systemParentName = A<string>();

            var systemUsageActive = A<bool>();
            var systemUsageExpirationDate = DateTime.Now.AddDays(-1);
            var systemUsageVersion = A<string>();
            var systemUsageLocalCallName = A<string>();
            var systemUsageLocalSystemId = A<string>();

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemParent = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemParentName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            // Role assignment
            var businessRoleDtos = await ItSystemUsageHelper.GetAvailableRolesAsync(organizationId);
            var role = businessRoleDtos.First();
            var availableUsers = await ItSystemUsageHelper.GetAvailableUsersAsync(organizationId);
            var user = availableUsers.First();
            using var assignRoleResponse = await ItSystemUsageHelper.SendAssignRoleRequestAsync(systemUsage.Id, organizationId, role.Id, user.Id);
            Assert.Equal(HttpStatusCode.Created, assignRoleResponse.StatusCode);

            // System changes
            await ItSystemHelper.SendSetDisabledRequestAsync(system.Id, systemDisabled);
            await ItSystemHelper.SendSetParentSystemRequestAsync(system.Id, systemParent.Id, organizationId);
            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organizationId, organizationId); // Using default organization as BelongsTo

            var availableBusinessTypeOptions = (await ItSystemHelper.GetBusinessTypeOptionsAsync(organizationId)).ToList();
            var businessType = availableBusinessTypeOptions[Math.Abs(A<int>()) % availableBusinessTypeOptions.Count];
            await ItSystemHelper.SendSetBusinessTypeRequestAsync(system.Id, businessType.Id, organizationId);

            var taskRefs = (await ItSystemHelper.GetAvailableTaskRefsRequestAsync(system.Id)).ToList();
            var taskRef = taskRefs[Math.Abs(A<int>()) % taskRefs.Count];
            await ItSystemHelper.SendAddTaskRefRequestAsync(system.Id, taskRef.TaskRef.Id, organizationId);

            // System Usage changes
            var body = new
            {
                Active = systemUsageActive,
                ExpirationDate = systemUsageExpirationDate,
                Version = systemUsageVersion,
                LocalCallName = systemUsageLocalCallName,
                LocalSystemId = systemUsageLocalSystemId
            };
            await ItSystemUsageHelper.PatchSystemUsage(systemUsage.Id, organizationId, body);

            // Responsible Organization Unit
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationId, organizationId); //Adding default organization as organization unit
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationId); //Using default organization as responsible organization unit

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            // From System Usage
            Assert.Equal(systemUsage.Id, readModel.SourceEntityId);
            Assert.Equal(organizationId, readModel.OrganizationId);
            Assert.Equal(systemUsageActive, readModel.IsActive);
            Assert.Equal(systemUsageVersion, readModel.Version);
            Assert.Equal(systemUsageLocalCallName, readModel.LocalCallName);

            // From System
            Assert.Equal(systemName, readModel.Name);
            Assert.Equal(systemDisabled, readModel.ItSystemDisabled);
            Assert.Equal(system.Uuid.ToString("D"), readModel.ItSystemUuid);
            Assert.Equal(businessType.Id, readModel.ItSystemBusinessTypeId);
            Assert.Equal(businessType.Name, readModel.ItSystemBusinessTypeName);
            Assert.Equal(organizationId, readModel.ItSystemRightsHolderId);
            Assert.Equal(organizationName, readModel.ItSystemRightsHolderName);
            Assert.Equal(taskRef.TaskRef.TaskKey, readModel.ItSystemKLEIdsAsCsv);
            Assert.Equal(taskRef.TaskRef.Description, readModel.ItSystemKLENamesAsCsv);

            // From Parent System
            Assert.Equal(systemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Id, readModel.ParentItSystemId);

            // Role assignment
            var roleAssignment = Assert.Single(readModel.RoleAssignments);
            Console.Out.WriteLine("Found one role assignment as expected");

            Assert.Equal(role.Id, roleAssignment.RoleId);
            Assert.Equal(user.Id, roleAssignment.UserId);
            Assert.Equal(user.FullName, roleAssignment.UserFullName);
            Assert.Equal(user.Email, roleAssignment.Email);

            // Responsible Organization Unit
            Assert.Equal(organizationId, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(organizationName, readModel.ResponsibleOrganizationUnitName);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_Today()
        {
            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(DateTime.Now);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_True_When_ExpirationDate_Is_After_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.True(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_IsActive_Is_False_When_ExpirationDate_Is_Earlier_Than_Today()
        {
            //Arrange
            var expirationDate = DateTime.Now.AddDays(-A<int>());

            //Act
            var readModel = await Test_For_IsActive_Based_On_ExpirationDate(expirationDate);

            //Assert
            Assert.False(readModel.IsActive);
        }

        [Fact]
        public async Task ReadModels_ItSystemParentName_Is_Null_When_No_Parent()
        {
            //Arrange
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Null(readModel.ParentItSystemName);
            Assert.Null(readModel.ParentItSystemId);
        }

        [Fact]
        public async Task ReadModels_ItSystemParentName_Is_Updated_When_Parent_Name_Is_Updated()
        {
            //Arrange
            var systemName = A<string>();
            var systemParentName = A<string>();
            var newSystemParentName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemParent = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemParentName, organizationId, AccessModifier.Public);
            await ItSystemHelper.SendSetParentSystemRequestAsync(system.Id, systemParent.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemHelper.SendSetNameRequestAsync(systemParent.Id, newSystemParentName, organizationId);
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(newSystemParentName, readModel.ParentItSystemName);
            Assert.Equal(systemParent.Id, readModel.ParentItSystemId);
        }

        [Fact]
        public async Task ReadModels_ResponsibleOrganizationUnit_Is_Updated_When_ResponsibleOrganizationUnit_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var orgUnitName1 = A<string>();
            var orgUnitName2 = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var organizationUnit1 = await OrganizationHelper.SendCreateOrganizationUnitRequestAsync(organizationId, orgUnitName1);
            var organizationUnit2 = await OrganizationHelper.SendCreateOrganizationUnitRequestAsync(organizationId, orgUnitName2);

            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id, organizationId);
            await ItSystemUsageHelper.SendAddOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id, organizationId);
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit1.Id); 

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await ItSystemUsageHelper.SendSetResponsibleOrganizationUnitRequestAsync(systemUsage.Id, organizationUnit2.Id);
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(organizationUnit2.Id, readModel.ResponsibleOrganizationUnitId);
            Assert.Equal(orgUnitName2, readModel.ResponsibleOrganizationUnitName);
        }

        [Fact]
        public async Task ReadModels_ItSystemRightsHolderName_Is_Updated_When_OrganizationName_Is_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var organizationName1 = A<string>();
            var organizationName2 = A<string>();
            var defaultOrganizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, defaultOrganizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, defaultOrganizationId);

            var organization1 = await OrganizationHelper.CreateOrganizationAsync(defaultOrganizationId, organizationName1, "", OrganizationTypeKeys.Kommune, AccessModifier.Public);

            await ItSystemHelper.SendSetBelongsToRequestAsync(system.Id, organization1.Id, defaultOrganizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await OrganizationHelper.SendChangeOrganizationNameRequestAsync(organization1.Id, organizationName2, defaultOrganizationId);
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(defaultOrganizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(organizationName2, readModel.ItSystemRightsHolderName);
        }

        [Fact]
        public async Task ReadModels_ItSystemBusinessTypeName_Is_Updated_When_BusinessType_Has_Its_Name_Changed()
        {
            //Arrange
            var systemName = A<string>();
            var businessTypeName1 = A<string>();
            var businessTypeName2 = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;

            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            var businessType = await EntityOptionHelper.SendCreateBusinessTypeAsync(businessTypeName1, organizationId);

            await ItSystemHelper.SendSetBusinessTypeRequestAsync(system.Id, businessType.Id, organizationId);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");

            //Act 
            await EntityOptionHelper.SendChangeBusinessTypeNameAsync(businessType.Id, businessTypeName2);
            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");
            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            //Assert
            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            Assert.Equal(businessTypeName2, readModel.ItSystemBusinessTypeName);
        }


        private async Task<ItSystemUsageOverviewReadModel> Test_For_IsActive_Based_On_ExpirationDate(DateTime expirationDate)
        {
            var systemName = A<string>();
            var organizationId = TestEnvironment.DefaultOrganizationId;
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(systemName, organizationId, AccessModifier.Public);
            var systemUsage = await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);
            await ItSystemUsageHelper.SendSetExpirationDateRequestAsync(systemUsage.Id, organizationId, expirationDate);

            //Wait for read model to rebuild (wait for the LAST mutation)
            await WaitForReadModelQueueDepletion();
            Console.Out.WriteLine("Read models are up to date");


            var readModels = (await ItSystemUsageHelper.QueryReadModelByNameContent(organizationId, systemName, 1, 0)).ToList();

            var readModel = Assert.Single(readModels);
            Console.Out.WriteLine("Read model found");

            return readModel;
        }

        private static async Task WaitForReadModelQueueDepletion()
        {
            await WaitForAsync(
                () =>
                {
                    return Task.FromResult(
                        DatabaseAccess.MapFromEntitySet<PendingReadModelUpdate, bool>(x => !x.AsQueryable().Any()));
                }, TimeSpan.FromSeconds(30));
        }

        private static async Task WaitForAsync(Func<Task<bool>> check, TimeSpan howLong)
        {
            bool conditionMet;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                conditionMet = await check();
            } while (conditionMet == false && stopwatch.Elapsed <= howLong);

            Assert.True(conditionMet, $"Failed to meet required condition within {howLong.TotalMilliseconds} milliseconds");
        }
    }
}
