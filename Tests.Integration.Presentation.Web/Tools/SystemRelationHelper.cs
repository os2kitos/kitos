using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.SystemRelations;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class SystemRelationHelper
    {
        public static SystemRelationDTO CreateSystemRelationDTO()
        {
            return new SystemRelationDTO();
        }

        public static async Task<IEnumerable<SystemRelationDTO>> SendGetRelationsAsync(int systemUsageId, Cookie login = null)
        {
            login = login ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/systemrelations/from/{systemUsageId}");

            using (var response = await HttpApi.GetWithCookieAsync(url, login))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<SystemRelationDTO>>();
            }
        }

        public static async Task<HttpResponseMessage> SendGetRelationAsync(int systemUsageId, int relationId, Cookie login = null)
        {
            login = login ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/systemrelations/from/{systemUsageId}/{relationId}");

            return await HttpApi.GetWithCookieAsync(url, login);
        }
        
        public static async Task<HttpResponseMessage> SendPostRelationAsync(CreateSystemRelationDTO input, Cookie login = null)
        {
            login = login ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/v1/systemrelations");

            return await HttpApi.PostWithCookieAsync(url, login, input);
        }

        public static async Task<HttpResponseMessage> SendDeleteRelationAsync(int systemUsageId, int relationId, Cookie login = null)
        {
            login = login ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v1/systemrelations/from/{systemUsageId}/{relationId}");

            return await HttpApi.DeleteWithCookieAsync(url, login);
        }

        public static async Task<HttpResponseMessage> SendPatchRelationAsync(SystemRelationDTO input, Cookie login = null)
        {
            login = login ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/v1/systemrelations");

            return await HttpApi.PatchWithCookieAsync(url, login, input);
        }
    }
}
