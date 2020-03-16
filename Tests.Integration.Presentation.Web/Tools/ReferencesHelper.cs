using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ReferencesHelper
    {
        public static async Task<ExternalReferenceDTO> CreateReferenceAsync(
            string title,
            string externalReferenceId,
            string referenceUrl,
            Display display,
            Action<ExternalReferenceDTO> setTargetId,
            Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/reference");

            var body = new ExternalReferenceDTO
            {
                Title = title,
                ExternalReferenceId = externalReferenceId,
                URL = referenceUrl,
                Display = display
            };
            setTargetId(body);

            using (var response = await HttpApi.PostWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ExternalReferenceDTO>();
            }
        }

        public static async Task<HttpResponseMessage> DeleteReferenceAsync(int id)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/reference/{id}?organizationId=-1");

            return await HttpApi.DeleteWithCookieAsync(url, cookie);
        }
    }
}
