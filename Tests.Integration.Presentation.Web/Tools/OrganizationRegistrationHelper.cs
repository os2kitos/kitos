using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V1.Organizations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class OrganizationRegistrationHelper
    {
        public static async Task<OrganizationRegistrationDTO> GetRegistrationsAsync(int unitId, Cookie optionalLogin = null)
        {
            using var response = await SendGetRegistrationsAsync(unitId, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationRegistrationDTO>();
        }
        public static async Task<HttpResponseMessage> SendGetRegistrationsAsync(int unitId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organization-units/{unitId}");

            return await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
        }

        public static async Task DeleteSelectedRegistrationsAsync(int unitId, ChangeOrganizationRegistrationRequest body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organization-units/{unitId}");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task DeleteUnitWithRegistrationsAsync(int unitId, int organizationId, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organization-units/unit/{unitId}/{organizationId}");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task TransferRegistrationsAsync(int unitId, int targetUnitId, ChangeOrganizationRegistrationRequest body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organization-units/{unitId}/{targetUnitId}");

            using var response = await HttpApi.PutWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
