using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1;
using Presentation.Web.Models.API.V1.Organizations;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class OrganizationUnitHelper
    {
        public static async Task<OrgUnitDTO> GetOrganizationUnitsAsync(int orgId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/OrganizationUnit?organization={orgId}");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<OrgUnitDTO>();
        }

        public static async Task<OrgUnitDTO> PatchOrganizationUnitsAsync(
            int orgId,
            int unitId,
            int? parentChange = null,
            string nameChange = null,
            int? eanChange = null,
            string localIdChange = null,
            Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/organizationUnit/{unitId}?organizationId={orgId}");

            dynamic body = new ExpandoObject();
            if (parentChange.HasValue)
                body.parentId = parentChange.Value;
            if (nameChange != null)
                body.name = nameChange;
            if (eanChange.HasValue)
                body.ean = eanChange.Value;
            if (localIdChange != null)
                body.localId = localIdChange;

            using HttpResponseMessage response = await HttpApi.PatchWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<OrgUnitDTO>();
        }

        public static async Task<List<OrganizationUnitRole>> GetOrganizationUnitRolesAsync(Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl("odata/OrganizationUnitRoles");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadOdataListResponseBodyAsAsync<OrganizationUnitRole>();
        }

        public static async Task<UnitAccessRightsDTO> GetUnitAccessRights(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}/access-rights");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<UnitAccessRightsDTO>();
        }

        public static async Task<List<UnitAccessRightsWithUnitIdDTO>> GetUnitAccessRightsForOrganization(Guid organizationUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/all/access-rights");

            using var response = await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
            var res = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<List<UnitAccessRightsWithUnitIdDTO>>();
        }

        public static async Task DeleteUnitAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
