using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ExpectedObjects;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.GDPR
{
    public class DataProcessingAgreementsTest : WithAutoFixture
    {
        [Fact]
        public async Task Can_Create()
        {
            //Arrange
            var name = A<string>();

            //Act
            var response = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(response);
        }

        [Fact]
        public async Task Can_Create_With_Same_Name_In_Different_Organizations()
        {
            //Arrange
            var name = A<string>();

            //Act
            var responseInFirstOrg = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);
            var responseInSecondOrg = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.SecondOrganizationId, name).ConfigureAwait(false);

            //Assert
            Assert.NotNull(responseInFirstOrg);
            Assert.NotNull(responseInSecondOrg);
            Assert.NotEqual(responseInFirstOrg.Id, responseInSecondOrg.Id);
            Assert.Equal(responseInFirstOrg.Name, responseInSecondOrg.Name);
        }

        [Fact]
        public async Task Cannot_Create_With_Duplicate_Name_In_Same_Organization()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            //Act
            await DataProcessingAgreementHelper.CreateAsync(organizationId, name).ConfigureAwait(false);
            using var secondResponse = await DataProcessingAgreementHelper.SendCreateRequestAsync(organizationId, name).ConfigureAwait(false);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Get()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name).ConfigureAwait(false);

            //Act
            var gotten = await DataProcessingAgreementHelper.GetAsync(dto.Id);

            //Assert
            Assert.NotNull(gotten);
            dto.ToExpectedObject().ShouldMatch(gotten);
        }

        [Fact]
        public async Task Can_Delete()
        {
            //Arrange
            var name = A<string>();
            var dto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name)
                .ConfigureAwait(false);

            //Act
            using var deleteResponse = await DataProcessingAgreementHelper.SendDeleteRequestAsync(dto.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task Can_Change_Name()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            var agreementDto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingAgreementHelper.SendChangeNameRequestAsync(agreementDto.Id, name2);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Change_Name_To_NonUniqueName_In_Same_Org()
        {
            //Arrange
            var name1 = A<string>();
            var name2 = A<string>();
            await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name1).ConfigureAwait(false);
            var agreementDto = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, name2).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingAgreementHelper.SendChangeNameRequestAsync(agreementDto.Id, name1);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Ok()
        {
            //Arrange
            var name = A<string>();

            //Act
            using var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Can_Create_Endpoint_Returns_Error_If_Non_Unique()
        {
            //Arrange
            var name = A<string>();
            const int organizationId = TestEnvironment.DefaultOrganizationId;

            await DataProcessingAgreementHelper.CreateAsync(organizationId, name).ConfigureAwait(false);

            //Act
            using var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(organizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData("")] //too short
        [InlineData("    ")] //only whitespace
        [InlineData("12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")] //101 chars
        public async Task Can_Create_Returns_Error_If_InvalidName(string name)
        {
            //Act
            using var response = await DataProcessingAgreementHelper.SendCanCreateRequestAsync(TestEnvironment.DefaultOrganizationId, name);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Query_ReadModels()
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
        public async Task Can_Get_Available_Roles()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act
            var roles = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);

            //Assert
            Assert.NotEmpty(roles);
        }

        [Fact]
        public async Task Can_Get_Available_Users()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();

            //Act
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);

            //Assert
            Assert.NotEmpty(availableUsers);
        }

        [Fact]
        public async Task Can_Assign_Role()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();

            //Act
            using var response = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            agreement = await DataProcessingAgreementHelper.GetAsync(agreement.Id);

            //Assert role is in the DTO
            var assignedRoleDto = Assert.Single(agreement.AssignedRoles);
            Assert.Equal(user.Id, assignedRoleDto.User.Id);
            Assert.Equal(role.Id, assignedRoleDto.Role.Id);

            //Assert query endpoint now excludes possible duplicate
            availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            Assert.Empty(availableUsers.Where(x => x.Id == user.Id));
        }

        [Fact]
        public async Task Cannot_Assign_Duplicate_Role()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var succeededResponse = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Act
            using var duplicateResponse = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, succeededResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        }

        [Fact]
        public async Task Cannot_Assign_Role_To_User_Not_In_Organization()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();

            //Act
            using var response = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, int.MaxValue);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Cannot_Assign_UnAvailable_Role()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());

            //Act
            using var response = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, int.MaxValue, TestEnvironment.DefaultUserId);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Can_Remove_Role()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();
            using var response = await DataProcessingAgreementHelper.SendAssignRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Act
            using var removeResponse = await DataProcessingAgreementHelper.SendRemoveRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);

            //Assert that the role is no longer in the DTO
            agreement = await DataProcessingAgreementHelper.GetAsync(agreement.Id);
            Assert.Empty(agreement.AssignedRoles);
        }

        [Fact]
        public async Task Cannot_Remove_Unassigned_Role()
        {
            //Arrange
            var agreement = await DataProcessingAgreementHelper.CreateAsync(TestEnvironment.DefaultOrganizationId, A<string>());
            var businessRoleDtos = await DataProcessingAgreementHelper.GetAvailableRolesAsync(agreement.Id);
            var role = businessRoleDtos.First();
            var availableUsers = await DataProcessingAgreementHelper.GetAvailableUsersAsync(agreement.Id, role.Id);
            var user = availableUsers.First();

            //Act - check on an agreement where no role has been added yet
            using var removeResponse = await DataProcessingAgreementHelper.SendRemoveRoleRequestAsync(agreement.Id, role.Id, user.Id);

            //Assert response
            Assert.Equal(HttpStatusCode.BadRequest, removeResponse.StatusCode);
        }
    }
}
