using System.Net;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceHelper
    {
        public static ItInterfaceDTO CreateInterfaceDto(
            string name,
            string interfaceId,
            int? userId,
            int orgId,
            AccessModifier access)
        {
            return new ItInterfaceDTO
            {
                ItInterfaceId = interfaceId,
                Name = name,
                OrganizationId = orgId,
                BelongsToId = userId,
                AccessModifier = access
            };
        }
        public static async Task CreateInterface(ItInterfaceDTO input)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/itinterface");

            using (var createdResponse = await HttpApi.PostWithCookieAsync(url, cookie, input))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                await createdResponse.ReadResponseBodyAs<ItInterfaceDTO>();
            }
        }

        public static async Task CreateInterfaces(params ItInterfaceDTO[] interfaces)
        {
            foreach (var dto in interfaces)
            {
                await CreateInterface(dto);
            }
        }

        public static async Task CreateItInterfaceUsageAsync(int itSystemUsageId, int interfaceId, int itSystemId, int organizationId, int contractId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var url = TestEnvironment.CreateUrl($"api/ItInterfaceUsage?usageId={itSystemUsageId}&interfaceId={interfaceId}&sysId={itSystemId}&organizationId={organizationId}");
            var body = new
            {
                itContractId = contractId
            };

            using (var createdResponse = await HttpApi.PatchWithCookieAsync(url, cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponse<ItInterfaceUsageDTO>();

                Assert.Equal(itSystemUsageId, response.ItSystemUsageId);
                Assert.Equal(interfaceId, response.ItInterfaceId);
                Assert.Equal(itSystemId, response.ItSystemId);
            }
        }
    }
}
