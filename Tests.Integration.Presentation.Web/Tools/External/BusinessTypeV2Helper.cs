using Core.DomainModel.Organization;
using Presentation.Web.Models.External.V2;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class BusinessTypeV2Helper
    {

        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetBusinessTypesAsync(Guid orgUuid, int pageSize, int pageNumber)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/business-types?organizationUuid={orgUuid}&pageSize={pageSize}&pageNumber={pageNumber}");
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<IEnumerable<IdentityNamePairResponseDTO>>();
            }
        }

        public static async Task<AvailableNamePairResponseDTO> GetBusinessTypeAsync(Guid businessTypeUuid, Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/business-types/{businessTypeUuid}?organizationUuid={organizationUuid}");
            using (var response = await HttpApi.GetWithCookieAsync(url, cookie))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsKitosApiResponseAsync<AvailableNamePairResponseDTO>();
            }
        }
    }
}
