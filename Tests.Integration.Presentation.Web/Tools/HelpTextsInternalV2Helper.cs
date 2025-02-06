using Core.DomainModel.Organization;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Request;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class HelpTextsInternalV2Helper
    {
        private const string Endpoint = "api/v2/internal/help-texts";

        public static async Task<HttpResponseMessage> GetAll()
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl(Endpoint), cookie);
        }

        public static async Task<HttpResponseMessage> GetSingle(string key)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{Endpoint}/{key}"), cookie);
        }

        public static async Task<HttpResponseMessage> Delete(string key)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(
                TestEnvironment.CreateUrl($"{Endpoint}/{key}"), cookie);
        }

        public static async Task<HttpResponseMessage> Create(HelpTextCreateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl(Endpoint), cookie, dto);
        }

        public static async Task<HttpResponseMessage> Patch(string key, HelpTextUpdateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"{Endpoint}/{key}"), cookie, dto);
        }
    }
}
