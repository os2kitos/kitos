using Core.DomainModel.Organization;
using Presentation.Web.Models.External.V2;
using Presentation.Web.Models.External.V2.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class OptionV2ApiHelper
    {
        public static class ResourceName
        {
            public const string BusinessType = "business-types";
            public const string ItSystemUsageDataClassification = "it-system-usage-data-classifications";
            public const string ItSystemUsageRelationFrequencies = "it-system-usage-relation-frequencies";
            public const string ItSystemUsageArchiveTypes = "it-system-usage-archive-types";
        }

        public static async Task<IEnumerable<IdentityNamePairResponseDTO>> GetOptionsAsync(string resource, Guid orgUuid, int pageSize, int pageNumber)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/{resource}?organizationUuid={orgUuid}&pageSize={pageSize}&page={pageNumber}");
            
            using var response = await HttpApi.GetWithTokenAsync(url, token.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<IdentityNamePairResponseDTO>>();
        }

        public static async Task<AvailableNamePairResponseDTO> GetOptionAsync(string resource, Guid businessTypeUuid, Guid organizationUuid)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"api/v2/{resource}/{businessTypeUuid}?organizationUuid={organizationUuid}");
            
            using var response = await HttpApi.GetWithTokenAsync(url, token.Token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<AvailableNamePairResponseDTO>();
        }
    }
}
