using Presentation.Web.Models;
using Presentation.Web.Models.API.V2.Request;
using Presentation.Web.Models.API.V2.Response.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.Interface;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public class InterfaceV2Helper
    {
        public static async Task<IEnumerable<RightsHolderItInterfaceResponseDTO>> GetRightsholderInterfacesAsync(string token, int? pageSize = null, int? pageNumber = null, Guid? rightsHolder = null, bool? includeDeactivated = null)
        {
            using var response = await SendGetRightsholderInterfacesAsync(token, pageSize, pageNumber, rightsHolder, includeDeactivated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<RightsHolderItInterfaceResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetRightsholderInterfacesAsync(string token, int? pageSize = null, int? pageNumber = null, Guid? rightsHolder = null, bool? includeDeactivated = null)
        {
            var path = "api/v2/rightsholder/it-interfaces";
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (pageNumber.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", pageNumber.Value.ToString("D")));

            if (rightsHolder.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("rightsHolderUuid", rightsHolder.Value.ToString("D")));

            if (includeDeactivated.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("includeDeactivated", includeDeactivated.Value.ToString()));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }

        public static async Task<RightsHolderItInterfaceResponseDTO> GetRightsholderInterfaceAsync(string token, Guid interfaceGuid)
        {
            using var response = await SendGetRightsholderInterfaceAsync(token, interfaceGuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItInterfaceResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetRightsholderInterfaceAsync(string token, Guid interfaceGuid)
        {
            var url = TestEnvironment.CreateUrl($"api/v2/rightsholder/it-interfaces/{interfaceGuid}");
            return await HttpApi.GetWithTokenAsync(url, token);
        }

        public static async Task<IEnumerable<ItInterfaceResponseDTO>> GetStakeholderInterfacesAsync(string token, int? pageSize = null, int? pageNumber = null, Guid? exposedBySystemUuid = null, bool? includeDeactivated = null)
        {
            using var response = await SendGetStakeholderInterfacesAsync(token, pageSize, pageNumber, exposedBySystemUuid, includeDeactivated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItInterfaceResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetStakeholderInterfacesAsync(string token, int? pageSize = null, int? pageNumber = null, Guid? exposedBySystemUuid = null, bool? includeDeactivated = null)
        {
            var path = "api/v2/it-interfaces";
            var queryParameters = new List<KeyValuePair<string, string>>();

            if (pageSize.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("pageSize", pageSize.Value.ToString("D")));

            if (pageNumber.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("page", pageNumber.Value.ToString("D")));

            if (exposedBySystemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("exposedBySystemUuid", exposedBySystemUuid.Value.ToString("D")));

            if (includeDeactivated.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("includeDeactivated", includeDeactivated.Value.ToString()));

            if (queryParameters.Any())
                path += $"?{string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"))}";

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl(path), token);
        }

        public static async Task<ItInterfaceResponseDTO> GetStakeholderInterfaceAsync(string token, Guid interfaceGuid)
        {
            using var response = await SendGetStakeholderInterfaceAsync(token, interfaceGuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItInterfaceResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetStakeholderInterfaceAsync(string token, Guid interfaceGuid)
        {
            var url = TestEnvironment.CreateUrl($"api/v2/it-interfaces/{interfaceGuid}");
            return await HttpApi.GetWithTokenAsync(url, token);
        }

        public static async Task<RightsHolderItInterfaceResponseDTO> CreateRightsHolderItInterfaceAsync(string token, RightsHolderCreateItInterfaceRequestDTO request)
        {
            using var response = await SendCreateRightsHolderItInterfaceAsync(token, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItInterfaceResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendCreateRightsHolderItInterfaceAsync(string token, RightsHolderCreateItInterfaceRequestDTO request)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl("api/v2/rightsholder/it-interfaces"), request, token);

        }

        public static async Task<RightsHolderItInterfaceResponseDTO> UpdateRightsHolderItInterfaceAsync(string token, Guid itInterfaceUuid, RightsHolderWritableItInterfacePropertiesDTO request)
        {
            using var response = await SendUpdateRightsHolderItInterfaceAsync(token, itInterfaceUuid, request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<RightsHolderItInterfaceResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendUpdateRightsHolderItInterfaceAsync(string token, Guid itInterfaceUuid, RightsHolderWritableItInterfacePropertiesDTO request)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/rightsholder/it-interfaces/{itInterfaceUuid}"), token, request);
        }

        public static async Task<HttpResponseMessage> SendDeleteRightsHolderItInterfaceAsync(string token, Guid itInterfaceUuid, DeactivationReasonRequestDTO request)
        {
            return await HttpApi.DeleteWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/rightsholder/it-interfaces/{itInterfaceUuid}"), token, request);
        }
    }
}
