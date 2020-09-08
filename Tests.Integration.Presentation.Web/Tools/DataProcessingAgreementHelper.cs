using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.GDPR;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Presentation.Web.Models.GDPR;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class DataProcessingAgreementHelper
    {
        public static async Task<DataProcessingAgreementDTO> CreateAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            using (var createdResponse = await SendCreateRequestAsync(organizationId, name, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<DataProcessingAgreementDTO>();

                Assert.Equal(name, response.Name);

                return response;
            }
        }

        public static async Task<HttpResponseMessage> SendCreateRequestAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new CreateDataProcessingAgreementDTO
            {
                Name = name,
                OrganizationId = organizationId
            };

            return await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-agreement"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendDeleteRequestAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-agreement/{id}"), cookie);
        }

        public static async Task<DataProcessingAgreementDTO> GetAsync(int id, Cookie optionalLogin = null)
        {
            using (var response = await SendGetRequestAsync(id, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<DataProcessingAgreementDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendGetRequestAsync(int id, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-agreement/{id}"), cookie);
        }

        public static async Task<HttpResponseMessage> SendChangeNameRequestAsync(int id, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new SingleValueDTO<string> { Value = name };

            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-agreement/{id}/name"), cookie, body);
        }

        public static async Task<HttpResponseMessage> SendCanCreateRequestAsync(int organizationId, string name, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/data-processing-agreement?orgId={organizationId}&checkname={name}"), cookie);
        }

        public static async Task<IEnumerable<DataProcessingAgreementReadModel>> QueryReadModelByNameContent(int organizationId, string nameContent, int top, int skip, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using (var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/DataProcessingAgreementReadModels?$filter=contains(Name,'{nameContent}')&$top={top}&$skip={skip}&$orderBy=Name"), cookie))
            {
                Assert.Equal(HttpStatusCode.OK,response.StatusCode);
                return await response.ReadOdataListResponseBodyAsAsync<DataProcessingAgreementReadModel>();
            }
        }
    }
}
