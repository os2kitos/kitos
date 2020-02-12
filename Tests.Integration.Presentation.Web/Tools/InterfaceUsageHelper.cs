﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceUsageHelper
    {
        public static async Task<ItInterfaceUsageDTO> CreateAsync(int contractId, int usageId, int systemId, int interfaceId, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                itContractId = contractId,
            };

            using (var createdResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/ItInterfaceUsage?usageId={usageId}&sysId={systemId}&interfaceId={interfaceId}&organizationId={organizationId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceUsageDTO>();

                Assert.Equal(contractId, response.ItContractId);
                Assert.Equal(usageId, response.ItSystemUsageId);
                Assert.Equal(interfaceId, response.ItInterfaceId);
                Assert.Equal(systemId, response.ItSystemId);
                return response;
            }
        }

        public static async Task<ItInterfaceUsageDTO> GetItInterfaceUsage(int usageId, int systemId, int interfaceId, bool allowErrorResponse = false)
        {
            using (var response = await GetItInterfaceUsageResponse(usageId, systemId, interfaceId))
            {
                if (!allowErrorResponse)
                {
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                }
                return await response.ReadResponseBodyAsKitosApiResponseAsync<ItInterfaceUsageDTO>();
            }
        }

        public static async Task<HttpResponseMessage> GetItInterfaceUsageResponse(int usageId, int systemId, int interfaceId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var oldInterfaceUrl = TestEnvironment.CreateUrl($"api/ItInterfaceUsage?" +
                                                            $"usageId={usageId}&" +
                                                            $"sysId={systemId}&" +
                                                            $"interfaceId={interfaceId}");

            return await HttpApi.GetWithCookieAsync(oldInterfaceUrl, cookie);
        }
    }
}