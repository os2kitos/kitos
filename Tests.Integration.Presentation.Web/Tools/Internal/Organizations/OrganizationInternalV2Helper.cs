using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private const string MasterDataSuffix = "master-data";
        private const string RolesSuffix = "roles";
        private const string UiCustomizationSuffix = "ui-customization";
        private const string UIRootConfigSuffix = "ui-root-config";

        public static async Task<HttpResponseMessage> GetOrganizationUIRootConfig(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{UIRootConfigSuffix}"), cookie);
        }

        public static async Task<HttpResponseMessage> PatchOrganizationUIRootConfig(Guid organizationUuid, UIRootConfigUpdateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{UIRootConfigSuffix}"), cookie, dto);
        }

        public static async Task<HttpResponseMessage> GetOrganizationMasterData(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{MasterDataSuffix}"), cookie);
        }

        public static async Task<HttpResponseMessage> PatchOrganization(Guid organizationUuid, OrganizationUpdateRequestDTO dto)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/patch"), cookie, dto);
        }

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

        public static async Task<HttpResponseMessage> GetUIModuleCustomization(Guid organizationUuid, string moduleName)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{UiCustomizationSuffix}/{moduleName}"), cookie);
        }

        public static async Task<HttpResponseMessage> PutUIModuleCustomization(Guid organizationUuid, string moduleName, UIModuleCustomizationRequestDTO dto, Cookie cookie = null)
        {
            cookie ??= await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/{UiCustomizationSuffix}/{moduleName}"), cookie, dto);
        }
    }
}
