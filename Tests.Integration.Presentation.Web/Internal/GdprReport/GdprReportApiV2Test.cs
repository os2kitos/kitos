using Core.DomainModel.Organization;
using System.Threading.Tasks;
using Tests.Integration.Presentation.Web.Tools;
using Xunit;
using Tests.Integration.Presentation.Web.Tools.Internal;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Internal.Response;
using System;
using Tests.Integration.Presentation.Web.Tools.External;
using Presentation.Web.Models.API.V2.Request.System.Regular;
using Presentation.Web.Models.API.V2.Request.SystemUsage;
using Presentation.Web.Models.API.V2.Response.SystemUsage;
using System.Linq;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Tests.Integration.Presentation.Web.Internal.GdprReport
{
    public class GdprReportApiV2Test : BaseTest
    {

        [Fact]
        public async Task Can_Get_Gdpr_Report()
        {
            var org = await CreateOrganizationAsync();
            var gdprWriteRequest = await CreateUsage(org.Uuid);

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

        private async Task<GDPRWriteRequestDTO> CreateUsage(Guid organizationUuid)
        {
            var token = await HttpApi.GetTokenAsync(OrganizationRole.GlobalAdmin);

            var systemRequest = new CreateItSystemRequestDTO { OrganizationUuid = organizationUuid, Name = A<string>()};
            systemRequest.OrganizationUuid = organizationUuid;
            var response = await ItSystemV2Helper.SendCreateSystemAsync(token.Token, systemRequest);
            Assert.True(response.IsSuccessStatusCode);
            var system = await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();

            var gdprRequest = MakeGDPRRequest();
            var usageRequest = new CreateItSystemUsageRequestDTO { SystemUuid = system.Uuid, OrganizationUuid = organizationUuid, GDPR = gdprRequest };
            usageRequest.OrganizationUuid = organizationUuid;
            usageRequest.SystemUuid = system.Uuid;
            var usageResponse = await ItSystemUsageV2Helper.SendPostAsync(token.Token, usageRequest);
            Assert.True(usageResponse.IsSuccessStatusCode);
            await response.ReadResponseBodyAsAsync<ItSystemUsageResponseDTO>();

            return gdprRequest;
        }

        private GDPRWriteRequestDTO MakeGDPRRequest()
        {
            return new GDPRWriteRequestDTO
            {
                HostedAt = A<HostingChoice>(),
                RiskAssessmentConducted = A<YesNoDontKnowChoice>(),
                RiskAssessmentConductedDate = A<DateTime>(),
                BusinessCritical = A<YesNoDontKnowChoice>(),
                PlannedRiskAssessmentDate = A<DateTime>(),
                DPIAConducted = A<YesNoDontKnowChoice>(),
            };
        }
    }
}
