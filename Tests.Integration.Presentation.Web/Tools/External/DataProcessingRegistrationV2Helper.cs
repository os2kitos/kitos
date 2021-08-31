using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.DataProcessing;
using Presentation.Web.Models.API.V2.Response.DataProcessing;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class DataProcessingRegistrationV2Helper
    {
        public static async Task<IEnumerable<DataProcessingRegistrationResponseDTO>> GetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessorUuid = null, Guid? subDataProcessorUuid = null, bool? agreementConcluded = null)
        {
            using var response = await SendGetDPRsAsync(token, page, pageSize, organizationUuid, systemUuid, systemUsageUuid, dataProcessorUuid, subDataProcessorUuid, agreementConcluded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<DataProcessingRegistrationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRsAsync(string token, int page = 0, int pageSize = 10, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessorUuid = null, Guid? subDataProcessorUuid = null, bool? agreementConcluded = null)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if (organizationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.Value.ToString("D")));

            if (systemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUuid", systemUuid.Value.ToString("D")));

            if (systemUsageUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUsageUuid", systemUsageUuid.Value.ToString("D")));

            if (dataProcessorUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("dataProcessorUuid", dataProcessorUuid.Value.ToString("D")));

            if (subDataProcessorUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("subDataProcessorUuid", subDataProcessorUuid.Value.ToString("D")));

            if (agreementConcluded.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("agreementConcluded", agreementConcluded.Value.ToString()));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations?{query}"), token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> GetDPRAsync(string token, Guid uuid)
        {
            using var response = await SendGetDPRAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetDPRAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid:D}"), token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PostAsync(string token, CreateDataProcessingRegistrationRequestDTO payload)
        {
            using var response = await SendPostAsync(token, payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPostAsync(string token, CreateDataProcessingRegistrationRequestDTO payload)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations"), payload, token);
        }

        public static async Task<DataProcessingRegistrationResponseDTO> PutAsync(string token, Guid uuid, DataProcessingRegistrationWriteRequestDTO payload)
        {
            using var response = await SendPutAsync(token, uuid, payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<DataProcessingRegistrationResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutAsync(string token, Guid uuid, DataProcessingRegistrationWriteRequestDTO payload)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}"), token, payload);
        }

        public static async Task<HttpResponseMessage> SendPutGeneralDataAsync(string token, Guid uuid, DataProcessingRegistrationGeneralDataWriteRequestDTO payload)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/data-processing-registrations/{uuid}/general"), token, payload);
        }
    }
}