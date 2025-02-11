using System;
using Presentation.Web.Models.API.V2.Internal.Response.ExternalReferences;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.References
{
    public class ExternalReferencesInternalV2Helper
    {
        private const string ControllerPrefix = "api/v2/internal/external-references";

        public static async Task<IEnumerable<ExternalReferenceWithLastChangedResponseDTO>> GetItSystemReferences(
            Guid systemUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var url = TestEnvironment.CreateUrl($"{ControllerPrefix}/it-systems/{systemUuid}");
            var response = await HttpApi.GetWithCookieAsync(url, cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ExternalReferenceWithLastChangedResponseDTO>>();
        }


    }
}
