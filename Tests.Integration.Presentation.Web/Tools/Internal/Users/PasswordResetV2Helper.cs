using Presentation.Web.Models.API.V2.Internal.Request;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Users
{
    public class PasswordResetV2Helper
    {
        private static readonly string routePrefix = "api/v2/internal/users/password-reset";

        public static async Task<HttpResponseMessage> PostPasswordReset(RequestPasswordResetRequestDTO request)
        {
            var url = TestEnvironment.CreateUrl($"{routePrefix}/create");
            return await HttpApi.PostAsync(url, request);
        }
    }
}
