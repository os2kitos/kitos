﻿using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.Contract;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItContractV2Helper
    {
        public static async Task<IEnumerable<ItContractResponseDTO>> GetItContractsAsync(string token, Guid organizationUuid, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessingRegistrationUuid = null, string nameContent = null, int page = 0, int pageSize = 10)
        {
            using var response = await SendGetItContractsAsync(token, organizationUuid, systemUuid, systemUsageUuid, dataProcessingRegistrationUuid, nameContent, page, pageSize);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItContractResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetItContractsAsync(string token, Guid organizationUuid, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessingRegistrationUuid = null, string nameContent = null, int page = 0, int pageSize = 10)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.ToString("D")));

            if (systemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUuid", systemUuid.Value.ToString("D")));

            if (systemUsageUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUsageUuid", systemUsageUuid.Value.ToString("D")));

            if (dataProcessingRegistrationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("dataProcessingRegistrationUuid", dataProcessingRegistrationUuid.Value.ToString("D")));

            if (nameContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContent", nameContent));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts?{query}"), token);
        }

        public static async Task<ItContractResponseDTO> GetItContractAsync(string token, Guid uuid)
        {
            using var response = await SendGetItContractAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItContractResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetItContractAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{uuid:D}"), token);
        }

        public static async Task<HttpResponseMessage> SendPostContractAsync(string token, CreateNewContractRequestDTO dto)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts"), dto, token);
        }

        public static async Task<ItContractResponseDTO> PostContractAsync(string token, CreateNewContractRequestDTO dto)
        {
            using var result = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts"), dto, token);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            return await result.ReadResponseBodyAsAsync<ItContractResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutContractAsync(string token, Guid contractUuid, ContractWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}"), token, dto);
        }
    }
}
