using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemHelper
    {
        public static async Task<ItSystemDTO> CreateItSystemInInitialOrganizationAsync(string itSystemName, int orgId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            var itSystem = new
            {
                name = itSystemName,
                belongsToId = orgId,
                organizationId = orgId
            };

            using (var createdResponse = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl("api/itsystem"), cookie, itSystem))
            {
                Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
                var response = await createdResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemDTO>();

                Assert.Equal(1, response.OrganizationId);
                Assert.Equal(1, response.BelongsToId);
                Assert.Equal(itSystemName, response.Name);
                return response;
            }
        }
    }
}
