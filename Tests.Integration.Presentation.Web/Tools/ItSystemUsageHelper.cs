using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.Organization;
using CsvHelper;
using CsvHelper.Configuration;
using Presentation.Web.Models;
using Presentation.Web.Models.ItSystemUsage;
using Tests.Integration.Presentation.Web.ItSystem;
using Xunit;

namespace Tests.Integration.Presentation.Web.Tools
{
    public class ItSystemUsageHelper
    {
        public static async Task<IEnumerable<ItSystemUsageOverviewReadModel>> QueryReadModelByNameContent(int organizationId, string nameContent, int top, int skip, Cookie optionalLogin = null)
        {
            var cookie = optionalLogin ?? await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            using var response = await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"odata/Organizations({organizationId})/ItSystemUsageOverviewReadModels?$filter=contains(Name,'{nameContent}')&$top={top}&$skip={skip}&$orderBy=Name"), cookie);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return await response.ReadOdataListResponseBodyAsAsync<ItSystemUsageOverviewReadModel>();
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> AddSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/add"), cookie, sensitiveDataLevel))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> RemoveSensitiveDataLevel(int systemUsageId, SensitiveDataLevel sensitiveDataLevel)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/itsystemusage/{systemUsageId}/sensitivityLevel/remove"), cookie, sensitiveDataLevel))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> SendSetActiveRequestAsync(int systemUsageId, int orgId, bool systemUsageActive)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var body = new
            {
                Active = systemUsageActive
            };
            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itSystemUsage/{systemUsageId}?organizationId={orgId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }

        public static async Task<ItSystemUsageSensitiveDataLevelDTO> SendSetExpirationDateRequestAsync(int systemUsageId, int orgId, DateTime systemUsageExpirationDate)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);
            var body = new
            {
                ExpirationDate = systemUsageExpirationDate
            };
            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itSystemUsage/{systemUsageId}?organizationId={orgId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageSensitiveDataLevelDTO>();
            }
        }


        public static async Task<ItSystemUsageDTO> PatchSystemUsage(int usageSystemId, int orgId, object body)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse = await HttpApi.PatchWithCookieAsync(TestEnvironment.CreateUrl($"api/itsystemusage/{usageSystemId}?organizationId={orgId}"), cookie, body))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                return await okResponse.ReadResponseBodyAsKitosApiResponseAsync<ItSystemUsageDTO>();
            }
        }

        public static async Task<List<GdprExportReportCsvFormat>> GetGDPRExportReport(int organizationId)
        {
            var cookie = await HttpApi.GetCookieAsync(OrganizationRole.GlobalAdmin);

            using (var okResponse =
                await HttpApi.GetWithCookieAsync(TestEnvironment.CreateUrl($"api/v1/gdpr-report/csv/{organizationId}"),
                    cookie))
            {
                Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
                using (var csvReader = new CsvReader(new StringReader(await okResponse.Content.ReadAsStringAsync()),
                    new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true
                    }))
                {
                    var report = csvReader.GetRecords<GdprExportReportCsvFormat>().ToList();
                    return report;
                }
            }
        }
    }
}