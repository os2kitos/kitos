using Core.DomainModel.Organization;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Tests.Integration.Presentation.Web.Tools.Internal;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Internal.Response;
using Tests.Integration.Presentation.Web.Tools.External;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using System.Linq;
using Presentation.Web.Models.API.V2.Response.Organization;
using Tests.Integration.Presentation.Web.SystemUsage.V2;

namespace Tests.Integration.Presentation.Web.Internal.GdprReport
{
    public class GdprReportApiV2Test : BaseItSystemUsageApiV2Test
    {

        [Fact]
        public async Task Can_Get_Gdpr_Report()
        {
            var org = await CreateOrganizationAsync();
            var gdprWriteRequest = await CreateUsage(org);

            var response = await GdprReportV2Helper.GetGdprReportAsync(org.Uuid);

            Assert.True(response.IsSuccessStatusCode);
            var body = await response.ReadResponseBodyAsAsync<IEnumerable<GdprReportResponseDTO>>();
            var report = body.FirstOrDefault();
            AssertGdprReportIsEqual(gdprWriteRequest, report);
        }

        private static void AssertGdprReportIsEqual(GDPRWriteRequestDTO dto, GdprReportResponseDTO? report)
        {
            Assert.Equal(dto.RiskAssessmentConducted, report?.RiskAssessment);
            AssertTimestampEqual(dto.RiskAssessmentConductedDate, report?.RiskAssessmentDate);
            AssertTimestampEqual(dto.PlannedRiskAssessmentDate, report?.PlannedRiskAssessmentDate);
            Assert.Equal(dto.DPIAConducted, report?.DPIA);
        }

        private static void AssertTimestampEqual(System.DateTime? expected, System.DateTime? actual)
        {
            Assert.Equal(NormalizeTimestamp(expected), NormalizeTimestamp(actual));
        }

        private static System.DateTime? NormalizeTimestamp(System.DateTime? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            var utcValue = value.Value.Kind switch
            {
                System.DateTimeKind.Utc => value.Value,
                System.DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => System.DateTime.SpecifyKind(value.Value, System.DateTimeKind.Utc)
            };

            var truncatedTicks = utcValue.Ticks - (utcValue.Ticks % 10);
            return new System.DateTime(truncatedTicks, System.DateTimeKind.Utc);
        }

        private async Task<GDPRWriteRequestDTO> CreateUsage(ShallowOrganizationResponseDTO organization)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var systemRequest = new CreateItSystemRequestDTO { OrganizationUuid = organization.Uuid, Name = A<string>() };
            var response = await ItSystemV2Helper.SendCreateSystemAsync(token.Token, systemRequest);
            Assert.True(response.IsSuccessStatusCode);
            var system = await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();

            var gdprRequest = await CreateGDPRInputAsync(organization);
            var usageRequest = new CreateItSystemUsageRequestDTO { SystemUuid = system.Uuid, OrganizationUuid = organization.Uuid, GDPR = gdprRequest };
            usageRequest.SystemUuid = system.Uuid;
            var usageResponse = await ItSystemUsageV2Helper.SendPostAsync(token.Token, usageRequest);
            Assert.True(usageResponse.IsSuccessStatusCode);
            await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();

            return gdprRequest;
        }
    }
}
