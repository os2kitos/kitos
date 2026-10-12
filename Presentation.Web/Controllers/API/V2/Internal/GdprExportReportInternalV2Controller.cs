using Core.ApplicationServices.SystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.GDPR;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Internal.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using System.Net;
namespace Presentation.Web.Controllers.API.V2.Internal
{

    [Route("api/v2/internal/gdpr-report")]
    public class GdprExportReportInternalV2Controller : InternalApiV2Controller
    {
        private readonly IGDPRExportService _gdprExportService;

        public GdprExportReportInternalV2Controller(IGDPRExportService gdprExportService)
        {
            _gdprExportService = gdprExportService;
        }

        [HttpGet]
        [Route("{organizationUuid}")]
        [ApiResponse(typeof(IEnumerable<GdprReportResponseDTO>), HttpStatusCode.OK)]
        [ApiResponse(HttpStatusCode.BadRequest)]
        [ApiResponse(HttpStatusCode.NotFound)]
        [ApiResponse(HttpStatusCode.Forbidden)]
        [ApiResponse(HttpStatusCode.Unauthorized)]
        public IActionResult GetGdprReport([FromRoute][NonEmptyGuid] Guid organizationUuid)
        {
            return _gdprExportService.GetGDPRDataByUuid(organizationUuid)
                    .Select(MapGdprDataToDTO)
                    .Match(Ok, FromOperationError);
        }

        private IEnumerable<GdprReportResponseDTO> MapGdprDataToDTO(IEnumerable<GDPRExportReport> gdprData)
        {
            return gdprData.Select(MapGdprReportToDTO).ToList();
        }

        private GdprReportResponseDTO MapGdprReportToDTO(GDPRExportReport gdprReport)
        {
            return new GdprReportResponseDTO
            {
                SystemUuid = new Guid(gdprReport.SystemUuid),
                SystemName = gdprReport.SystemName,
                NoData = gdprReport.NoData,
                PersonalData = gdprReport.PersonalData,
                SensitiveData = gdprReport.SensitiveData,
                LegalData = gdprReport.LegalData,
                DataProcessingAgreementConcluded = gdprReport.DataProcessingAgreementConcluded,
                LinkToDirectory = gdprReport.LinkToDirectory,
                SensitiveDataTypes = gdprReport.SensitiveDataTypes,
                RiskAssessment = gdprReport.RiskAssessment?.ToChoice(),
                RiskAssessmentDate = gdprReport.RiskAssessmentDate,
                PlannedRiskAssessmentDate = gdprReport.PlannedRiskAssessmentDate,
                PreRiskAssessment = gdprReport.PreRiskAssessment?.ToRiskLevelChoice(),
                RiskAssessmentNotes = gdprReport.RiskAssessmentNotes,
                PersonalDataCpr = gdprReport.PersonalDataCpr,
                PersonalDataSocialProblems = gdprReport.PersonalDataSocialProblems,
                PersonalDataSocialOtherPrivateMatters = gdprReport.PersonalDataSocialOtherPrivateMatters,
                DPIA = gdprReport.DPIA?.ToYesNoDontKnowChoice(),
                DPIADate = gdprReport.DPIADate,
                TechnicalSupervisionDocumentationUrl = gdprReport.TechnicalSupervisionDocumentationUrl,
                TechnicalSupervisionDocumentationUrlName = gdprReport.TechnicalSupervisionDocumentationUrlName,
                UserSupervision = gdprReport.UserSupervision?.ToYesNoDontKnowChoice(),
                UserSupervisionDocumentationUrl = gdprReport.UserSupervisionDocumentationUrl,
                UserSupervisionDocumentationUrlName = gdprReport.UserSupervisionDocumentationUrlName,
                NextDataRetentionEvaluationDate = gdprReport.NextDataRetentionEvaluationDate,
                InsecureCountriesSubjectToDataTransfer = gdprReport.InsecureCountriesSubjectToDataTransfer
            };
        }
    }
}

