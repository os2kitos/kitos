using Presentation.Web.Models;
using Presentation.Web.Models.External.V2.Response;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public class InterfaceV2Helper
    {
        public static async Task<IEnumerable<ItInterfaceResponseDTO>> GetRightsholderInterfacesAsync(LoginDTO user, int pageSize, int pageNumber)
        {
            var token = await HttpApi.GetTokenAsync(user);
            var url = TestEnvironment.CreateUrl($"api/v2/rightsholder/it-interfaces?pageSize={pageSize}&page={pageNumber}");
            using (var response = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsAsync<IEnumerable<ItInterfaceResponseDTO>>();
            }
        }

        public static async Task<ItInterfaceResponseDTO> GetRightsholderInterfaceAsync(LoginDTO user, Guid interfaceGuid)
        {
            var token = await HttpApi.GetTokenAsync(user);
            var url = TestEnvironment.CreateUrl($"api/v2/rightsholder/it-interfaces/{interfaceGuid}");
            using (var response = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsAsync<ItInterfaceResponseDTO>();
            }
        }

        public static async Task<IEnumerable<ItInterfaceResponseDTO>> GetStakeholderInterfacesAsync(LoginDTO user, int pageSize, int pageNumber)
        {
            var token = await HttpApi.GetTokenAsync(user);
            var url = TestEnvironment.CreateUrl($"api/v2/it-interfaces?pageSize={pageSize}&page={pageNumber}");
            using (var response = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsAsync<IEnumerable<ItInterfaceResponseDTO>>();
            }
        }

        public static async Task<ItInterfaceResponseDTO> GetStakeholderInterfaceAsync(LoginDTO user, Guid interfaceGuid)
        {
            var token = await HttpApi.GetTokenAsync(user);
            var url = TestEnvironment.CreateUrl($"api/v2/it-interfaces/{interfaceGuid}");
            using (var response = await HttpApi.GetWithTokenAsync(url, token.Token))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                return await response.ReadResponseBodyAsAsync<ItInterfaceResponseDTO>();
            }
        }
    }
}
