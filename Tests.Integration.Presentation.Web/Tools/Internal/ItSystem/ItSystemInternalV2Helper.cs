

using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Options;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystem
{
    public static class ItSystemInternalV2Helper
    {
        private const string ItSystemsApiPrefix = "api/v2/internal/it-systems";
        private const string LocalOptionTypesSuffix = "local-option-types";

        public static async Task<HttpResponseMessage> GetLocalOptionTypes(Guid organizationUuid, string choiceTypeName)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{ItSystemsApiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie);
        }

        public static async Task<HttpResponseMessage> GetLocalOptionTypeByOptionId(Guid organizationUuid, string choiceTypeName, Guid optionUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{ItSystemsApiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }

        public static async Task<HttpResponseMessage> CreateLocalOptionType(Guid organizationUuid, string choiceTypeName, LocalRegularOptionCreateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl($"{ItSystemsApiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> PatchLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, LocalRegularOptionUpdateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"{ItSystemsApiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> DeleteLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(
                TestEnvironment.CreateUrl($"{ItSystemsApiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }
    }
}
