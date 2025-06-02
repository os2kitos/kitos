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
            Assert.NotEmpty(body);
            var report = body.FirstOrDefault();
            AssertGdprReportIsEqual(gdprWriteRequest, report);
        }

        private void AssertGdprReportIsEqual(GDPRWriteRequestDTO dto, GdprReportResponseDTO report)
        {
            Assert.Equal(dto.HostedAt, report.HostedAt);
            Assert.Equal(dto.RiskAssessmentConducted, report.RiskAssessment);
            Assert.Equal(dto.RiskAssessmentConductedDate, report.RiskAssessmentDate);
            Assert.Equal(dto.BusinessCritical, report.BusinessCritical);
            Assert.Equal(dto.PlannedRiskAssessmentDate, report.PlannedRiskAssessmentDate);
            Assert.Equal(dto.DPIAConducted, report.DPIA);
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
