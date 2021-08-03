using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models;
using Presentation.Web.Models.API.V1;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class AuthorizationHelper
    {
        public static async Task<UserDTO> GetUser(Cookie login)
        {
            using (var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/authorize"), login))
            {
                Assert.Equal(HttpStatusCode.OK,response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<UserDTO>();
            };
        }
    }
}
