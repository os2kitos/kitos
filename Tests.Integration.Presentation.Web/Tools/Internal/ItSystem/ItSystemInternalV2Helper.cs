

using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystem
{
    public static class ItSystemInternalV2Helper
    {
        public static async Task<HttpResponseMessage> GetLocalOptionTypes(Guid organizationUuid, string choiceTypeName)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"api/v2/internal/it-systems/{organizationUuid}/local-choice-types/{choiceTypeName}"), cookie);
        }

        public static async Task<HttpResponseMessage> GetLocalOptionTypeByOptionId(Guid organizationUuid, string choiceTypeName, int optionId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"api/v2/internal/it-systems/{organizationUuid}/local-choice-types/{choiceTypeName}/{optionId}"), cookie);
        }
    }
}
