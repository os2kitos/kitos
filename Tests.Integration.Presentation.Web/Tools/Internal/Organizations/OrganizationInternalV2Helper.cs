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
        private const string ApiPrefix = "api/v2/internal/organizations";
        private const string MasterDataSuffix = "masterData";
        private const string RolesSuffix = "roles";

        public static async Task<HttpResponseMessage> PatchOrganizationMasterData(Guid organizationUuid,
            OrganizationMasterDataRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{MasterDataSuffix}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> GetOrganizationMasterDataRoles(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{MasterDataSuffix}/{RolesSuffix}"), cookie);
        }

        public static async Task<HttpResponseMessage> PatchOrganizationMasterDataRoles(Guid organizationUuid,
            OrganizationMasterDataRolesRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{MasterDataSuffix}/{RolesSuffix}"), cookie, dto);
        }
    }
}
