using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationGridConfigurationApiV2Test : WithAutoFixture
    {

        [Fact]
        public async Task LocalAdminCanSaveGridConfig()
        {
            var (org, cookie) = await CreatePrerequisites();

            var columns = CreateTestColumns().ToList();
            var saveResponse = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, columns, cookie);
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanGetGridConfiguration()
        {
            //Admins saves a column configuration
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            _ = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, columns, localAdminCookie);

            //User fetches column configuration
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);
            var userResponse =
                await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(org.Uuid, userCookie);
            var userResponseBody = await userResponse.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();

            Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
            foreach (var column in userResponseBody.VisibleColumns)
            {
                Assert.Contains(column.PersistId, columns.Select(x => x.PersistId));
            }
        }

        [Fact]
        public async Task LocalAdminCanDeleteGridConfiguration()
        {
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            _ = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, columns, localAdminCookie);

            var deleteResponse = await 
                OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotSaveGridConfiguration()
        {
            var (org, _) = await CreatePrerequisites();
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            var response = await
                OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, CreateTestColumns(), userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotDeleteGridConfiguration()
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            _ = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid,
                CreateTestColumns(), localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            var response = await
                OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserSeesLocalAdminChanges()
        {
            //Local admin saves grid config
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            _ = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid,
                columns, localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            //User retrieves Grid Config
            var body = await OrganizationGridConfigurationTestHelper.GetResponseBodyAsync(org.Uuid, userCookie);
            foreach (var column in body.VisibleColumns)
            {
                Assert.Contains(column.PersistId, columns.Select(x => x.PersistId));
            }

            //Local admin deletes grid config
            _ = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);

            //Users tries to retrieve grid config after it has been deleted
            var response = await
                OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

            private IEnumerable<KendoColumnConfigurationDTO> CreateTestColumns()
        {
            var cols = new List<KendoColumnConfigurationDTO>
            {
                new ()
                {
                    PersistId = A<string>(),
                    Index = 1,
                }
            };
            return cols;

        }

        private async Task<(OrganizationDTO org, Cookie cookie)> CreatePrerequisites()
        {
            var org = await OrganizationHelper.CreateOrganizationAsync(TestEnvironment.DefaultOrganizationId, TestEnvironment.DefaultOrganizationName, String.Empty,
                OrganizationTypeKeys.Kommune, AccessModifier.Local);
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.LocalAdmin, org.Id);
            return (org, cookie);
        }

        private string CreateEmail()
        {
            return $"{nameof(OrganizationUnitTests)}{A<string>()}@test.dk";
        }
    }
}
