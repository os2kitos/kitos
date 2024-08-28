using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Core.DomainServices.Generic;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationGridConfigurationAPIV2Test { 

        private Guid defaultOrgUuid;

        public OrganizationGridConfigurationAPIV2Test(IEntityIdentityResolver entityIdentityResolver) {
            defaultOrgUuid = entityIdentityResolver.ResolveUuid<Organization>(TestEnvironment.DefaultOrganizationId).Value;
        }

        private List<KendoColumnConfigurationDTO> CreateTestColumns()
        {
            return new List<KendoColumnConfigurationDTO> {
                new KendoColumnConfigurationDTO
                {
                    PersistId = "SystemName",
                    Index = 1,
                }
            };
        }

        [Fact]
        public async Task RegularUserCanGetGridConfiguration()
        {
            Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
            var response = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
            var responseBody = await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(responseBody);
            Assert.NotNull(responseBody.VisibleColumns);
        }

        [Fact]
        public async Task LocalAdminCanSaveGridConfiguration()
        {
            var columns = CreateTestColumns();
            var saveResponse = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(defaultOrgUuid, 0, columns);
            var saveResponseBody = await saveResponse.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
            Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
            Assert.Contains("SystemName", saveResponseBody.VisibleColumns.Select(colConfig => colConfig.PersistId));
        }

        [Fact]
        public async Task LocalAdminCanDeleteGridConfiguration()
        {
            var columns = CreateTestColumns();
            var deleteResponse = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0);
            var deleteResponseBody = await deleteResponse.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO> ();
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            Assert.Empty(deleteResponseBody.VisibleColumns);
        } 

        [Fact]
        public async Task RegularUserCanNotSaveGridConfiguration()
        {
            Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
            var response = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(defaultOrgUuid, 0, CreateTestColumns(), regularUserCookie);
            Assert.Equal (HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotDeleteGridConfiguration()
        {
            Cookie regularUserCookie = await HttpApi.GetCookieAsync(OrganizationRole.User);
            var response = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }     

        [Fact]
        public async Task RegularUserSeesLocalAdminChanges()
        {
            //Local admin saves config
            var columns = CreateTestColumns();
            var saveResponse = await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(defaultOrgUuid, 0, columns);

            //Regular user retrieves columns
            Cookie regularUserCookie = await HttpApi.GetCookieAsync (OrganizationRole.User);
            var userResponseAfterSave = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
            var userResponseBodyAfterSave = await userResponseAfterSave.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO>();
            Assert.Contains("SystemName", userResponseBodyAfterSave.VisibleColumns.Select(x => x.PersistId));

            //Local admin deletes config
            var deleteResponse = await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(defaultOrgUuid, 0);

            //Regular user retrieves columns after local admin deleted them 
            var userResponseAfterDelete = await OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(defaultOrgUuid, 0, regularUserCookie);
            var userResponseBodyAfterDelete = await userResponseAfterDelete.ReadResponseBodyAsKitosApiResponseAsync<OrganizationGridConfigurationResponseDTO> ();
            Assert.Empty(userResponseBodyAfterDelete.VisibleColumns);

        }
    }
}
