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
        public async Task RegularUserCanGetGridConfiguration()
        {
            var (org, cookie) = await CreatePrerequisites();

            var response = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(org.Uuid, cookie);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task LocalAdminCanSaveGridConfig()
        {
            var (org, cookie) = await CreatePrerequisites();

            var columns = CreateTestColumns().ToList();
            var saveResponse = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, columns, cookie);
            var saveResponseBody = await saveResponse.ReadResponseBodyAsAsync<OrganizationGridConfigurationResponseDTO>();
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
            foreach (var column in columns)
            {
                Assert.Contains(column.PersistId, saveResponseBody.VisibleColumns.Select(colConfig => colConfig.PersistId));
                
            }
        }

        //[Fact]
        //public async Task LocalAdminCanDeleteGridConfiguration()
        //{
        //    var columns = CreateTestColumns();
        //    var deleteResponse = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0);
        //    var deleteResponseBody = await deleteResponse.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
        //    Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        //    Assert.Empty(deleteResponseBody.VisibleColumns);
        //}

        //[Fact]
        //public async Task RegularUserCanNotSaveGridConfiguration()
        //{
        //    Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
        //    var response = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(defaultOrgUuid, 0, CreateTestColumns(), regularUserCookie);
        //    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        //}

        //[Fact]
        //public async Task RegularUserCanNotDeleteGridConfiguration()
        //{
        //    Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
        //    var response = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
        //    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        //}

        //[Fact]
        //public async Task RegularUserSeesLocalAdminChanges()
        //{
        //    Local admin saves config
        //    var columns = CreateTestColumns();
        //    var saveResponse = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(defaultOrgUuid, 0, columns);

        //    Regular user retrieves columns
        //    Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
        //    var userResponseAfterSave = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
        //    var userResponseBodyAfterSave = await userResponseAfterSave.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
        //    Assert.Contains("SystemName", userResponseBodyAfterSave.VisibleColumns.Select(x => x.PersistId));

        //    Local admin deletes config
        //    var deleteResponse = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0);

        //    Regular user retrieves columns after local admin deleted them
        //    var userResponseAfterDelete = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
        //    var userResponseBodyAfterDelete = await userResponseAfterDelete.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
        //    Assert.Empty(userResponseBodyAfterDelete.VisibleColumns);

        //}
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
                OrganizationTypeKeys.Kommune, AccessModifier.Public);  //Not sure about these parameters, but i guess they aren't too important for what i am testing
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.GlobalAdmin, org.Id);
            return (org, cookie);
        }

        private string CreateEmail()
        {
            return $"{nameof(OrganizationUnitTests)}{A<string>()}@test.dk";
        }
    }
}
