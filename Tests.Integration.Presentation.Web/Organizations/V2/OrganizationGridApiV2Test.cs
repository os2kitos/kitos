using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Tests.Integration.Presentation.Web.Tools;
using Tests.Toolkit.Patterns;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationGridApiV2Test : WithAutoFixture
    {

        [Fact]
        public async Task LocalAdminCanDeleteGridConfiguration()
        {
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridTestHelper.SaveConfigurationRequestAsync(org.Uuid, columns, localAdminCookie);

            using var deleteResponse = await 
                OrganizationGridTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotSaveGridConfiguration()
        {
            var (org, _) = await CreatePrerequisites();
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            using var response = await
                OrganizationGridTestHelper.SendSaveConfigurationRequestAsync(org.Uuid, CreateTestColumns(), userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotDeleteGridConfiguration()
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridTestHelper.SendSaveConfigurationRequestAsync(org.Uuid,
                CreateTestColumns(), localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            using var response = await
                OrganizationGridTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserSeesLocalAdminChanges()
        {
            //Local admin saves grid config
            var columns = CreateTestColumns().ToList();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridTestHelper.SaveConfigurationRequestAsync(org.Uuid,
                columns, localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);

            //User retrieves Grid Config
            var body = await OrganizationGridTestHelper.GetResponseBodyAsync(org.Uuid, userCookie);
            AssertColumnsMatch(columns, body.VisibleColumns.ToList());
            

            //Local admin deletes grid config
            await OrganizationGridTestHelper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);

            //Users tries to retrieve grid config after it has been deleted
            using var response = await
                OrganizationGridTestHelper.SendGetConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task LocalAdminHasConfigModificationPermissions()
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            var permissions = await
                OrganizationGridTestHelper.GetOrganizationPermissionsAsync(org.Uuid, localAdminCookie);
            Assert.True(permissions.HasConfigModificationPermissions);
        }

        [Fact]
        public async Task GlobalAdminHasDoesNotHaveConfigModificationPermissions()
        {
            var globalAdminCookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var (org, _) = await CreatePrerequisites();
            var permissions = await
                OrganizationGridTestHelper.GetOrganizationPermissionsAsync(org.Uuid, globalAdminCookie);
            Assert.False(permissions.HasConfigModificationPermissions);
        }

        [Fact]
        public async Task RegularUserDoesNotHaveConfigModificationPermissions()
        {
            var (org, _) = await CreatePrerequisites();
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Id);
            var permissions =
                await OrganizationGridTestHelper.GetOrganizationPermissionsAsync(org.Uuid, userCookie);
            Assert.False(permissions.HasConfigModificationPermissions);
        }


            private IEnumerable<ColumnConfigurationRequestDTO> CreateTestColumns()
            {
                var cols = new List<ColumnConfigurationRequestDTO>();
                for (int i = 0; i < 10; i++)
                {
                    cols.Add(new ColumnConfigurationRequestDTO()
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

        private static void AssertColumnsMatch(List<ColumnConfigurationRequestDTO> requestedColumns,
            List<ColumnConfigurationResponseDTO> responseColumns)
        {
            foreach (var columnTuple in requestedColumns.Select(column => (column.Index, column.PersistId)))
            {
                Assert.Contains(columnTuple, responseColumns.Select(col => (col.Index, col.PersistId)));
            }
        }
        
    }
}
