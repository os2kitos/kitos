using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class ItContractHelper
    {
        public static async Task<ItContractDTO> CreateContract(string name, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                name = name,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itcontract"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItContractDTO>();

                Assert.Equal(organizationId, response.OrganizationId);
                Assert.Equal(name, response.Name);
                return response;
            }
        }

        public static async Task<HttpResponseMessage> SendDeleteContractRequestAsync(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            return await HttpApi.DeleteWithCookieAsync(
                TestEnvironment.CreateUrl($"api/itcontract/{contractId}?{KitosApiConstants.UnusedOrganizationIdParameter}"), cookie);
        }

        public static async Task<ItContractDTO> GetItContract(int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/ItContract/{contractId}");
            using (var result = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                var contract = await result.ReadResponseBodyAsKitosApiResponseAsync<ItContractDTO>();
                return contract;
            }
        }

        public static async Task<ItSystemUsageSimpleDTO> AddItSystemUsage(int contractId, int usageId, int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var body = new
            {
                systemUsageId = usageId,
                organizationId = organizationId,
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"api/itcontract/{contractId}?systemUsageId={usageId}&organizationId={organizationId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<ItSystemUsageSimpleDTO>>();

                var addedMapping = response.FirstOrDefault(x => x.Id == usageId);
                Assert.NotNull(addedMapping);
                return addedMapping;
            }
        }

        public static async Task<HandoverTrialDTO> AddHandOverTrialAsync(int contractId, DateTime approved, DateTime expected, Cookie optionalLogin = null)
        {
            using (var response = await SendAddHandOverTrialAsync(contractId, approved, expected, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<HandoverTrialDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddHandOverTrialAsync(int contractId, DateTime approved, DateTime expected, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/handoverTrial/");
            var body = new
            {
                approved = approved,
                expected = expected,
                handoverTrialTypeId = "1",
                itContractId = contractId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<PaymentMilestoneDTO> AddPaymentMilestoneAsync(int contractId, DateTime approved, DateTime expected, string title, Cookie optionalLogin = null)
        {
            using (var response = await SendAddPaymentMilestoneRequestAsync(contractId, approved, expected, title, optionalLogin))
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<PaymentMilestoneDTO>();
            }
        }

        public static async Task<HttpResponseMessage> SendAddPaymentMilestoneRequestAsync(int contractId, DateTime approved, DateTime expected, string title, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl("api/paymentMilestone/");
            var body = new
            {
                approved = approved,
                expected = expected,
                title = title,
                itContractId = contractId
            };

            return await HttpApi.PostWithCookieAsync(url, cookie, body);
        }

        public static async Task<ItContractDTO> PatchContract(int contractId, int orgId, object body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"/api/itcontract/{contractId}?organizationId={orgId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItContractDTO>();
            }
        }
    }
}