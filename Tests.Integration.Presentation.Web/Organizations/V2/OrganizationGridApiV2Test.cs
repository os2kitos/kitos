using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;

namespace Tests.Integration.Presentation.Web.Organizations.V2
{
    public class OrganizationGridApiV2Test : BaseTest
    {

        [Fact]
        public async Task LocalAdminCanDeleteGridConfiguration()
        {
            var columns = CreateTestColumns().ToArray();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigV2Helper.SaveConfigurationRequestAsync(org.Uuid, columns, localAdminCookie);

            using var deleteResponse = await
                OrganizationGridConfigV2Helper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotSaveGridConfiguration()
        {
            var (org, _) = await CreatePrerequisites();
            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Uuid);

            using var response = await
                OrganizationGridConfigV2Helper.SendSaveConfigurationRequestAsync(org.Uuid, CreateTestColumns(), userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserCanNotDeleteGridConfiguration()
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigV2Helper.SendSaveConfigurationRequestAsync(org.Uuid,
                CreateTestColumns(), localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Uuid);

            using var response = await
                OrganizationGridConfigV2Helper.SendDeleteConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RegularUserSeesLocalAdminChanges()
        {
            //Local admin saves grid config
            var columns = CreateTestColumns().ToList();
            var (org, localAdminCookie) = await CreatePrerequisites();
            await OrganizationGridConfigV2Helper.SaveConfigurationRequestAsync(org.Uuid,
                columns, localAdminCookie);

            var (_, _, userCookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.User, org.Uuid);

            //User retrieves Grid Config
            var body = await OrganizationGridConfigV2Helper.GetResponseBodyAsync(org.Uuid, userCookie);
            AssertColumnsMatch(columns, body.VisibleColumns.ToList());


            //Local admin deletes grid config
            await OrganizationGridConfigV2Helper.SendDeleteConfigurationRequestAsync(org.Uuid, localAdminCookie);

            //Users tries to retrieve grid config after it has been deleted
            using var response = await
                OrganizationGridConfigV2Helper.SendGetConfigurationRequestAsync(org.Uuid, userCookie);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(OrganizationRole.User, false)]
        [InlineData(OrganizationRole.LocalAdmin, true)]
        [InlineData(OrganizationRole.GlobalAdmin, false)]
        public async Task PermissionToModifyGridConfigHasCorrectValueForRoles(OrganizationRole role, bool expected)
        {
            var (org, localAdminCookie) = await CreatePrerequisites();
            var cookie = (role == OrganizationRole.LocalAdmin) ? localAdminCookie : await HttpApi.GetCookieAsync(role);
            var permissions = await OrganizationGridConfigV2Helper.GetOrganizationPermissionsAsync(org.Uuid, cookie);
            Assert.Equal(permissions.HasConfigModificationPermissions, expected);
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

        private async Task<(ShallowOrganizationResponseDTO org, Cookie cookie)> CreatePrerequisites()
        {
            var org = await CreateOrganizationAsync();
            var (_, _, cookie) = await HttpApi.CreateUserAndLogin(CreateEmail(), OrganizationRole.LocalAdmin, org.Uuid);
            return (org, cookie);
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
