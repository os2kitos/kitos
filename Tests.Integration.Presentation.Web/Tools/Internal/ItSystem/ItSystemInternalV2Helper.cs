

using System.Net.Http;
using System.Threading.Tasks;
using System;
using Core.DomainModel.Organization;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystem
{
    public static class ItSystemInternalV2Helper
    {
        public static async Task<HttpResponseMessage> GetLocalChoiceTypes(Guid organizationUuid, string choiceTypeName)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(
                TestEnvironment.CreateUrl($"api/v2/internal/it-systems/{organizationUuid}/local-choice-types/{choiceTypeName}"), cookie);
        }
    }
}
