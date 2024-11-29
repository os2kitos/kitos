using Core.DomainModel.ItSystem.DataTypes;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        public bool PersonalDataCpr { get; set; }
        public bool PersonalDataSocialProblems { get; set; }
        public bool PersonalDataSocialOtherPrivateMatters { get; set; }
        public YesNoDontKnowChoice? DPIA { get; set; }
        public HostingChoice? HostedAt { get; set; }
    }
}