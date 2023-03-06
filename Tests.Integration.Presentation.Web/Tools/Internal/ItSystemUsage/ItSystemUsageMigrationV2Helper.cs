using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystemUsage
{
    public class ItSystemUsageMigrationV2Helper
    {
        const string BasePath = "api/v2/internal/it-system-usages";

        public static async Task<IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>> GetUnusedSystemsAsync(Guid organizationUuid, string nameContent, int numberOfItSystems, bool getPublicFromOtherOrganizations, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var path = BasePath + $"/unused";
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("organizationUuid", organizationUuid.ToString("D")),
                new("nameContent", nameContent),
                new("numberOfItSystems", numberOfItSystems.ToString("D")),
                new("getPublicFromOtherOrganizations", getPublicFromOtherOrganizations.ToString())
            };

            path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";


            var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>>();
        }
    }
}
