using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Options;
using System.Net.Http;
using System.Threading.Tasks;
using System;

namespace Tests.Integration.Presentation.Web.Tools.Internal
{
    public static class LocalOptionTypeV2Helper
    {
        private const string LocalOptionTypesSuffix = "local-option-types";

        public static async Task<HttpResponseMessage> GetLocalOptionTypes(Guid organizationUuid, string choiceTypeName, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie);
        }

        public static async Task<HttpResponseMessage> GetLocalOptionType(Guid organizationUuid, string choiceTypeName, Guid optionUuid, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }

        public static async Task<HttpResponseMessage> CreateLocalOptionType(Guid organizationUuid, string choiceTypeName, LocalOptionCreateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PostWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> PatchLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, LocalRegularOptionUpdateRequestDTO dto, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> DeleteLocalOptionType(Guid organizationUuid, Guid optionUuid, string choiceTypeName, string apiPrefix)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.DeleteWithCookieAsync(
                TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{LocalOptionTypesSuffix}/{choiceTypeName}/{optionUuid}"), cookie);
        }
    }
}
