using Core.DomainModel.Organization;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit;
using Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Organizations
{
    public class OrganizationUnitRegistrationV2Helper
    {
        public static async Task<OrganizationRegistrationUnitResponseDTO> GetRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            using var response = await SendGetRegistrationsAsync(organizationUuid, unitUuid, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<OrganizationRegistrationUnitResponseDTO>();
        }
        public static async Task<HttpResponseMessage> SendGetRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            return await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
        }

        public static async Task DeleteSelectedRegistrationsAsync(Guid organizationUuid, Guid unitUuid, ChangeOrganizationUnitRegistrationV2RequestDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task TransferRegistrationsAsync(Guid organizationUuid, Guid unitUuid, TransferOrganizationUnitRegistrationV2RequestDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            using var response = await HttpApi.PutWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
