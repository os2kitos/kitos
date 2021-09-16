using Presentation.Web.Models.API.V2.Response.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Presentation.Web.Models.API.V2.Request.Contract;
using Presentation.Web.Models.API.V2.Types.Shared;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools.External
{
    public static class ItContractV2Helper
    {
        public static async Task<IEnumerable<ItContractResponseDTO>> GetItContractsAsync(string token, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessingRegistrationUuid = null, Guid? responsibleOrgUnitUuid = null, Guid? supplierUuid = null, string nameContent = null, int page = 0, int pageSize = 10)
        {
            using var response = await SendGetItContractsAsync(token, organizationUuid, systemUuid, systemUsageUuid, dataProcessingRegistrationUuid, responsibleOrgUnitUuid, supplierUuid, nameContent, page, pageSize);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<IEnumerable<ItContractResponseDTO>>();
        }

        public static async Task<HttpResponseMessage> SendGetItContractsAsync(string token, Guid? organizationUuid = null, Guid? systemUuid = null, Guid? systemUsageUuid = null, Guid? dataProcessingRegistrationUuid = null, Guid? responsibleOrgUnitUuid = null, Guid? supplierUuid = null, string nameContent = null, int page = 0, int pageSize = 10)
        {
            var queryParameters = new List<KeyValuePair<string, string>>()
            {
                new("page", page.ToString("D")),
                new("pageSize", pageSize.ToString("D")),
            };

            if(organizationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("organizationUuid", organizationUuid.Value.ToString("D")));

            if (systemUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUuid", systemUuid.Value.ToString("D")));

            if (systemUsageUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("systemUsageUuid", systemUsageUuid.Value.ToString("D")));

            if (dataProcessingRegistrationUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("dataProcessingRegistrationUuid", dataProcessingRegistrationUuid.Value.ToString("D")));

            if (responsibleOrgUnitUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("responsibleOrgUnitUuid", responsibleOrgUnitUuid.Value.ToString("D")));

            if (supplierUuid.HasValue)
                queryParameters.Add(new KeyValuePair<string, string>("supplierUuid", supplierUuid.Value.ToString("D")));

            if (nameContent != null)
                queryParameters.Add(new KeyValuePair<string, string>("nameContent", nameContent));

            var query = string.Join("&", queryParameters.Select(x => $"{x.Key}={x.Value}"));

            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts?{query}"), token);
        }

        public static async Task<ItContractResponseDTO> GetItContractAsync(string token, Guid uuid)
        {
            using var response = await SendGetItContractAsync(token, uuid);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            return await response.ReadResponseBodyAsAsync<ItContractResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendGetItContractAsync(string token, Guid uuid)
        {
            return await HttpApi.GetWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{uuid:D}"), token);
        }

        public static async Task<HttpResponseMessage> SendPostContractAsync(string token, CreateNewContractRequestDTO dto)
        {
            return await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts"), dto, token);
        }

        public static async Task<ItContractResponseDTO> PostContractAsync(string token, CreateNewContractRequestDTO dto)
        {
            using var result = await HttpApi.PostWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts"), dto, token);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            return await result.ReadResponseBodyAsAsync<ItContractResponseDTO>();
        }

        public static async Task<HttpResponseMessage> SendPutContractAsync(string token, Guid contractUuid, UpdateContractRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutContractGeneralDataAsync(string token, Guid contractUuid, ContractGeneralDataWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/general"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutContractResponsibleAsync(string token, Guid contractUuid, ContractResponsibleDataWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/responsible"), token, dto);
		}
		
        public static async Task<HttpResponseMessage> SendPutProcurementAsync(string token, Guid contractUuid, ContractProcurementDataWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/procurement"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutContractSupplierAsync(string token, Guid contractUuid, ContractSupplierDataWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/supplier"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutContractHandOverTrialsAsync(string token, Guid contractUuid, IEnumerable<HandoverTrialRequestDTO> request)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/handover-trials"), token, request);
		}
		
        public static async Task<HttpResponseMessage> SendPutSystemUsagesAsync(string token, Guid contractUuid, IEnumerable<Guid> dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/system-usages"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutExternalReferences(string token, Guid contractUuid, List<ExternalReferenceDataDTO> request)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/external-references"), token, request);
        }

        public static async Task<HttpResponseMessage> SendPutDataProcessingRegistrationsAsync(string token, Guid contractUuid, IEnumerable<Guid> dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/data-processing-registrations"), token, dto);
        }

        public static async Task<HttpResponseMessage> SendPutPaymentModelAsync(string token, Guid contractUuid, ContractPaymentModelDataWriteRequestDTO dto)
        {
            return await HttpApi.PutWithTokenAsync(TestEnvironment.CreateUrl($"api/v2/it-contracts/{contractUuid}/payment-model"), token, dto);
        }
    }
}
