using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Internal.Response.ItSystemUsage;
using Xunit;
using System.IO;

namespace Tests.Integration.Presentation.Web.Tools.Internal.ItSystemUsage
{
    public class ItSystemUsageMigrationV2Helper
    {
        const string BasePath = "api/v2/internal/it-system-usages";

        public static async Task<IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>> GetUnusedSystemsAsync(Guid organizationUuid, int numberOfItSystems, bool getPublicFromOtherOrganizations, string nameContent = null, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var path = BasePath + "/unused";
            var queryParameters = new List<KeyValuePair<string, string>>
            {
                new("organizationUuid", organizationUuid.ToString("D")),
                new("numberOfItSystems", numberOfItSystems.ToString("D")),
                new("getPublicFromOtherOrganizations", getPublicFromOtherOrganizations.ToString())
            };

            if(nameContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContent", nameContent));

            path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairWithDeactivatedStatusDTO>>();
        }

        public static async Task<ItSystemUsageMigrationV2ResponseDTO> GetMigration(Guid itSystemUsageUuid, Guid toSystemUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var path = BasePath + $"/{itSystemUsageUuid}/migration?toSystemUuid={toSystemUuid}";

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemUsageMigrationV2ResponseDTO>();
        }

        public static async Task<ItSystemUsageMigrationPermissionsResponseDTO> GetPermissions(Guid itSystemUsageUuid, Cookie userCookie = null)
        {
            var cookie = userCookie ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var path = BasePath + $"/{itSystemUsageUuid}/migration/permissions";

            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl(path), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItSystemUsageMigrationPermissionsResponseDTO>();
        }
    }
}
