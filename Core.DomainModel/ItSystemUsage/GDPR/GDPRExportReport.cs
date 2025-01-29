using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem.DataTypes;

namespace Core.DomainModel.ItSystemUsage.GDPR
{
    public class GDPRExportReport
    {
        public string SystemUuid { get; set; }
        public string SystemName { get; set; }
        public bool NoData { get; set; }
        public bool PersonalData { get; set; }
        public bool SensitiveData { get; set; }
        public bool LegalData { get; set; }
        public DataOptions? BusinessCritical { get; set; }
        public bool DataProcessingAgreementConcluded { get; set; }
        public bool LinkToDirectory { get; set; }
        public IEnumerable<string> SensitiveDataTypes { get; set; }
        public DataOptions? RiskAssessment { get; set; }
        public DateTime? RiskAssessmentDate { get; set; }
        public DateTime? PlannedRiskAssessmentDate { get; set; }
        public RiskLevel? PreRiskAssessment { get; set; }
        public string RiskAssessmentNotes { get; set; }
        public bool PersonalDataCpr { get; set; }
        public bool PersonalDataSocialProblems { get; set; }
        public bool PersonalDataSocialOtherPrivateMatters { get; set; }
        public DataOptions? DPIA { get; set; }
        public DateTime? DPIADate { get; set; }
        public HostedAt? HostedAt { get; set; }
        public string TechnicalSupervisionDocumentationUrlName { get; set; }
        public string TechnicalSupervisionDocumentationUrl { get; set; }
        public DataOptions? UserSupervision { get; set; }
        public string UserSupervisionDocumentationUrl { get; set; }
        public string UserSupervisionDocumentationUrlName { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        public IEnumerable<string> InsecureCountriesSubjectToDataTransfer { get; set; }


    }
}
