using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.DomainModel.ItSystemUsage.GDPR;
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
