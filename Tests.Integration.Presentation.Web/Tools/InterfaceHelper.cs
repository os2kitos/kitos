using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public static class InterfaceHelper
    {
        public static ItInterfaceDTO CreateInterfaceDTO(string name, string ItInterfaceId, int? userID, int orgId, AccessModifier access)
        {
            return new ItInterfaceDTO
            {
                ItInterfaceId = ItInterfaceId,
                Name = name,
                OrganizationId = orgId,
                BelongsToId = userID,
                AccessModifier = access
            };
        }
        public static async Task CreateInterface(ItInterfaceDTO iDto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl("api/itinterface");

            using (var createdResponse = await HttpApi.PostWithCookieAsync(url, cookie, iDto))
            {
             //   Assert.Equal(HttpStatusCode.Created,createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAs<ItInterfaceDTO>();
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
