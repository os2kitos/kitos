using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Response
{
    public class GdprReportResponseDTO
    {
        public Guid SystemUuid { get; set; }
        public string SystemName { get; set; }
        public bool NoData { get; set; }
        public bool PersonalData { get; set; }
        public bool SensitiveData { get; set; }
        public bool LegalData { get; set; }
        public YesNoDontKnowChoice? BusinessCritical { get; set; }
        public bool DataProcessingAgreementConcluded { get; set; }
        public bool LinkToDirectory { get; set; }
        public IEnumerable<string> SensitiveDataTypes { get; set; }
        public YesNoDontKnowChoice? RiskAssessment { get; set; } 
        public DateTime? RiskAssessmentDate { get; set; }
        public DateTime? PlannedRiskAssessmentDate { get; set; }
        public RiskLevelChoice? PreRiskAssessment { get; set; } 
        public string RiskAssessmentNotes { get; set; }
        public bool PersonalDataCpr { get; set; }
        public bool PersonalDataSocialProblems { get; set; }
        public bool PersonalDataSocialOtherPrivateMatters { get; set; }
        public YesNoDontKnowChoice? DPIA { get; set; }
        public DateTime? DPIADate { get; set; }
        public HostingChoice? HostedAt { get; set; }
        public string TechnicalSupervisionDocumentationUrlName { get; set; }
        public string TechnicalSupervisionDocumentationUrl { get; set; }
        public YesNoDontKnowChoice? UserSupervision { get; set; }
        public string UserSupervisionDocumentationUrl { get; set; }
        public string UserSupervisionDocumentationUrlName { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }

        public IEnumerable<string> InsecureCountriesSubjectToDataTransfer { get; set; }
    }
}