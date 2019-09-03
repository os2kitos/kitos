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
    }
}
