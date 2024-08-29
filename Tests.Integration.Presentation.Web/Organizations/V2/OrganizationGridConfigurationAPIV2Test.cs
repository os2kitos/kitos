using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
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
        public async Task LocalAdminCanDeleteGridConfiguration()
        {
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigurationTestHelper.SaveConfigurationRequestAsync(org.Uuid, columns, localAdminCookie);

            using var deleteResponse = await 
                OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotSaveGridConfiguration()
        {
            var (org, _) = await CreatePrerequisites();
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            using var response = await
                OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, CreateTestColumns(), userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotDeleteGridConfiguration()
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigurationTestHelper.SendSaveConfigurationRequestAsync(org.Uuid,
                CreateTestColumns(), localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            using var response = await
                OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserSeesLocalAdminChanges()
        {
            //Local admin saves grid config
            var columns = CreateTestColumns().ToList();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigurationTestHelper.SaveConfigurationRequestAsync(org.Uuid,
                columns, localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            //User retrieves Grid Config
            var body = await OrganizationGridConfigurationTestHelper.GetResponseBodyAsync(org.Uuid, userCookie);
            AssertColumnsMatch(columns, body.VisibleColumns.ToList());
            

            //Local admin deletes grid config
            await OrganizationGridConfigurationTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);

            //Users tries to retrieve grid config after it has been deleted
            using var response = await
                OrganizationGridConfigurationTestHelper.SendGetConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

            private IEnumerable<KendoColumnConfigurationDTO> CreateTestColumns()
            {
                var cols = new List<KendoColumnConfigurationDTO>();
                for (int i = 0; i < 10; i++)
                {
                    cols.Add(new KendoColumnConfigurationDTO
                    {
                        Index = i,
                        PersistId = A<string>()
                    });
                }
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

        private void AssertColumnsMatch(List<KendoColumnConfigurationDTO> requestedColumns,
            List<ColumnConfigurationResponseDTO> responseColumns)
        {
            foreach (var columnTuple in requestedColumns.Select(column => (column.Index, column.PersistId)))
            {
                Assert.Contains(columnTuple, responseColumns.Select(col => (col.Index, col.PersistId)));
            }
        }
        
    }
}
