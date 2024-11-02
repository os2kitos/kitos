

using Core.DomainModel.Organization;
using System.Net.Http;
using System.Threading.Tasks;
using System;

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
    }
}
