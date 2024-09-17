using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Request.Organization;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Organizations
{
    internal class OrganizationInternalV2Helper
    {
        public static async Task<HttpResponseMessage> PatchOrganizationMasterData(Guid organizationUuid,
            OrganizationMasterDataRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v2/internal/organizations/{organizationUuid}"), cookie, dto);
        }
    }
}
