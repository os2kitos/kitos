using Core.DomainModel.Organization;
using Presentation.Web.Models.API.V2.Internal.Response.Organizations.Suppliers;
using Presentation.Web.Models.API.V2.Request.Supplier;
using Presentation.Web.Models.API.V2.Response.Organization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.Internal.Organizations
{
    public class OrganizationSupplierInternalV2Helper
    {
        private const string ApiPrefix = "api/v2/internal/organizations";
        
        public static async Task<ShallowOrganizationResponseDTO> AddSupplier(Guid organizationUuid, Guid supplierUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.PostWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers/{supplierUuid}"), cookie, null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<ShallowOrganizationResponseDTO>();
        }

        public static async Task DeleteSupplier(Guid organizationUuid, Guid supplierUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.DeleteWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers/{supplierUuid}"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static async Task<IEnumerable<ShallowOrganizationResponseDTO>> GetSuppliers(Guid organizationUuid)
        {
            using var response = await SendGetSuppliers(organizationUuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ShallowOrganizationResponseDTO>>();
        }
        public static async Task<IEnumerable<ShallowOrganizationResponseDTO>> GetAvailableSuppliers(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers/available"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<ShallowOrganizationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetSuppliers(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers"), cookie);
        }

        public static async Task<IEnumerable<SupplierAssociatedFieldConfigurationResponseDTO>> GetSupplierFields(Guid organizationUuid)
        {
            using var response = await SendGetSupplierFields(organizationUuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<SupplierAssociatedFieldConfigurationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetSupplierFields(Guid organizationUuid)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers/fields"), cookie);
        }

        public static async Task<IEnumerable<SupplierAssociatedFieldConfigurationResponseDTO>> PutSupplierFields(Guid organizationUuid, SupplierAssociatedFieldConfigurationRequestDTO configuration)
        {
            using var response = await SendPutSupplierFields(organizationUuid, configuration);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadResponseBodyAsAsync<IEnumerable<SupplierAssociatedFieldConfigurationResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendPutSupplierFields(Guid organizationUuid, SupplierAssociatedFieldConfigurationRequestDTO configuration)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            return await HttpApi.PutWithCookieAsync(TestEnvironment.CreateUrl($"{ApiPrefix}/{organizationUuid}/suppliers/fields"), cookie, configuration);
        }
    }
}
