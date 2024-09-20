using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Request.Organizations;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Organizations
{
    internal class OrganizationInternalV2Helper
    {
        private const string apiPrefix = "api/v2/internal/organizations";
        private const string masterDataSuffix = "masterData";
        private const string rolesSuffix = "roles";

        public static async Task<HttpResponseMessage> PatchOrganizationMasterData(Guid organizationUuid,
            OrganizationMasterDataRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{masterDataSuffix}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> GetOrganizationMasterDataRoles(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{masterDataSuffix}/{rolesSuffix}"), cookie);
        }

        public static async Task<HttpResponseMessage> PatchOrganizationMasterDataRoles(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{apiPrefix}/{organizationUuid}/{masterDataSuffix}/{rolesSuffix}"), cookie, dto);
        }
    }
}
