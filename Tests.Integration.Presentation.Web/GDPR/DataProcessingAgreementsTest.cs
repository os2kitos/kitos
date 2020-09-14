using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.GDPR.Read;
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

        [Fact]
        public async Task Can_Get_Available_Systems()
        {
            //Arrange
            var systemPrefix = A<Guid>().ToString("N");
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system1Name = $"{systemPrefix}{1}";
            var system2Name = $"{systemPrefix}{2}";
            var filteredOutSystemName = A<string>();
            var agreement = await DataProcessingAgreementHelper.CreateAsync(organizationId, A<string>());
            var system1 = await ItSystemHelper.CreateItSystemInOrganizationAsync(system1Name, organizationId, AccessModifier.Public);
            var system2 = await ItSystemHelper.CreateItSystemInOrganizationAsync(system2Name, organizationId, AccessModifier.Public);
            var filteredOutSystem = await ItSystemHelper.CreateItSystemInOrganizationAsync(filteredOutSystemName, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system1.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(system2.Id, organizationId);
            await ItSystemHelper.TakeIntoUseAsync(filteredOutSystem.Id, organizationId);

            //Act
            var dtos = (await DataProcessingAgreementHelper.GetAvailableSystemsAsync(agreement.Id, systemPrefix)).ToList();

            //Assert
            Assert.Equal(2, dtos.Count);
            dtos.Select(x => new { x.Id, x.Name }).ToExpectedObject().ShouldMatch(new[] { new { system1.Id, system1.Name }, new { system2.Id, system2.Name } });
        }

        [Fact]
        public async Task Can_Assign_And_Remove_System()
        {
            //Arrange
            var systemPrefix = A<Guid>().ToString("N");
            const int organizationId = TestEnvironment.DefaultOrganizationId;
            var system1Name = $"{systemPrefix}{1}";
            var agreement = await DataProcessingAgreementHelper.CreateAsync(organizationId, A<string>());
            var system = await ItSystemHelper.CreateItSystemInOrganizationAsync(system1Name, organizationId, AccessModifier.Public);
            await ItSystemHelper.TakeIntoUseAsync(system.Id, organizationId);

            //Act - Add
            using var assignResponse = await DataProcessingAgreementHelper.SendAssignSystemRequestAsync(agreement.Id, system.Id);
            using var duplicateResponse = await DataProcessingAgreementHelper.SendAssignSystemRequestAsync(agreement.Id, system.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
            var dto = await DataProcessingAgreementHelper.GetAsync(agreement.Id);
            var systemDTO = Assert.Single(dto.ItSystems);
            Assert.Equal(system.Id, systemDTO.Id);
            Assert.Equal(system.Name, systemDTO.Name);

            //Act - remove
            using var removeResponse = await DataProcessingAgreementHelper.SendRemoveSystemRequestAsync(agreement.Id, system.Id);
            using var duplicateRemoveResponse = await DataProcessingAgreementHelper.SendRemoveSystemRequestAsync(agreement.Id, system.Id);

            //Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, duplicateRemoveResponse.StatusCode);
            dto = await DataProcessingAgreementHelper.GetAsync(agreement.Id);
            Assert.Empty(dto.ItSystems);
        }
    }
}
