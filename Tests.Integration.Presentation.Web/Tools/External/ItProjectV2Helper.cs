﻿using Presentation.Web.Models.External.V2.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItProjectV2Helper
    {
        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetItProjectsAsync(string token, Guid organizationUuid, string nameContent = null, int page = 0, int pageSize = 10)
        {
            using var response = await SendGetItProjectsAsync(token, organizationUuid, nameContent, page, pageSize);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetItProjectsAsync(string token, Guid organizationUuid, string nameContent = null, int page = 0, int pageSize = 10)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.ToString("D")));

            if (nameContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContent", nameContent));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-projects?{query}"), token);
        }

        public static async Task<IdentityNamePairResponseDTO> GetItProjectAsync(string token, Guid uuid)
        {
            using var response = await SendGetItProjectAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IdentityNamePairResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetItProjectAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-projects/{uuid:D}"), token);
        }
    }
}