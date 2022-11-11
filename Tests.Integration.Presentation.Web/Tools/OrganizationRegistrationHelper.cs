using System;
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
        public static async Task<OrganizationRegistrationUnitDTO> GetRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            using var response = await SendGetRegistrationsAsync(organizationUuid, unitUuid, optionalLogin);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsKitosApiResponseAsync<OrganizationRegistrationUnitDTO>();
        }
        public static async Task<HttpResponseMessage> SendGetRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            return await HttpApi.GetWithCookieAsync(orgUnitUrl, cookie);
        }

        public static async Task DeleteSelectedRegistrationsAsync(Guid organizationUuid, Guid unitUuid, ChangeOrganizationUnitRegistrationRequestDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task DeleteUnitWithRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}");

            using var response = await HttpApi.DeleteWithCookieAsync(orgUnitUrl, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task TransferRegistrationsAsync(Guid organizationUuid, Guid unitUuid, Guid targetUnitUuid, TransferOrganizationUnitRegistrationRequestDTO body, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var orgUnitUrl = TestEnvironment.CreateUrl($"api/v1/organizations/{organizationUuid}/organization-units/{unitUuid}/registrations");

            using var response = await HttpApi.PutWithCookieAsync(orgUnitUrl, cookie, body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
