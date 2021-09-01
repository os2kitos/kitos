using Core.DomainModel;
using Core.DomainModel.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Integration.Presentation.Web.Tools.External;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Projects.V2
{
    public class ItProjectsApiV2Test : WithAutoFixture
    {
        [Fact]
        public async Task GET_Project_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newProject = await ItProjectHelper.CreateProject(A<string>(), TestEnvironment.DefaultOrganizationId);

            //Act
            var dto = await ItProjectV2Helper.GetItProjectAsync(regularUserToken.Token, newProject.Uuid);

            //Assert
            Assert.Equal(newProject.Uuid, dto.Uuid);
            Assert.Equal(newProject.Name, dto.Name);
        }

        [Fact]
        public async Task GET_Project_Returns_NotFound()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItProjectV2Helper.SendGetItProjectAsync(regularUserToken.Token, A<Guid>());

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GET_Project_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            using var response = await ItProjectV2Helper.SendGetItProjectAsync(regularUserToken.Token, Guid.Empty);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Projects_Returns_Ok()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var projects = await ItProjectV2Helper.GetItProjectsAsync(regularUserToken.Token, defaultOrgUuid, null, 0, 100);

            //Assert
            Assert.NotEmpty(projects);
        }

        [Fact]
        public async Task GET_Projects_Returns_Ok_With_Name_Content_Filtering()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var newProject = await ItProjectHelper.CreateProject(A<string>(), TestEnvironment.DefaultOrganizationId);
            var defaultOrgUuid = DatabaseAccess.GetEntityUuid<Organization>(TestEnvironment.DefaultOrganizationId);

            //Act
            var projects = await ItProjectV2Helper.GetItProjectsAsync(regularUserToken.Token, defaultOrgUuid, newProject.Name, 0, 100);

            //Assert
            var project = Assert.Single(projects);
            Assert.Equal(newProject.Uuid, project.Uuid);
        }

        [Fact]
        public async Task GET_Projects_Returns_BadRequest_For_Empty_Uuid()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);

            //Act
            var response = await ItProjectV2Helper.SendGetItProjectsAsync(regularUserToken.Token, Guid.Empty, null, 0, 100);

            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_Projects_Returns_Forbidden_For_Organization_Where_User_Has_No_Roles()
        {
            //Arrange
            var regularUserToken = await HttpApi.GetTokenAsync(OrganizationRole.User);
            var organization = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId,
                A<string>(), string.Join("", Many<int>(8).Select(x => Math.Abs(x) % 9)), A<OrganizationTypeKeys>(), AccessModifier.Public);

            //Act
            var response = await ItProjectV2Helper.SendGetItProjectsAsync(regularUserToken.Token, organization.Uuid, null, 0, 100);

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
