

using Core.DomainModel.Organization;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Tests.Integration.Presentation.Web.Tools.Internal
{
    public class GlobalOptionTypeV2Helper
    {
        private const string GlobalOptionTypesSuffix = "global-option-types";

        public static async Task<HttpResponseMessage> GetGlobalOptionTypes(string choiceTypeName, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{GlobalOptionTypesSuffix}/{choiceTypeName}"), cookie);
        }

        public static async Task<HttpResponseMessage> CreateGlobalOptionType(string choiceTypeName, GlobalRegularOptionCreateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{GlobalOptionTypesSuffix}/{choiceTypeName}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> PatchGlobalOptionType(Guid optionUuid, string choiceTypeName, GlobalRegularOptionUpdateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{GlobalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie, dto);
        }
    }
}
