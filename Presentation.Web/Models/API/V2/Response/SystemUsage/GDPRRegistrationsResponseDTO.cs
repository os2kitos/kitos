using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Response.SystemUsage
{
    public class GDPRRegistrationsResponseDTO
    {
        public string Purpose { get; set; }
        public YesNoDontKnowChoice? BusinessCritical { get; set; }
        public HostingChoice? HostedAt { get; set; }
        public SimpleLinkDTO DirectoryDocumentation { get; set; }
        public IEnumerable<DataSensitivityLevelChoice> DataSensitivityLevels { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> SensitivePersonData { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> RegisteredDataCategories { get; set; }
        public YesNoDontKnowChoice? TechnicalPrecautionsInPlace { get; set; }
        public IEnumerable<TechnicalPrecautionChoice> TechnicalPrecautionsApplied { get; set; }
        public SimpleLinkDTO TechnicalPrecautionsDocumentation { get; set; }
        public YesNoDontKnowChoice? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public SimpleLinkDTO UserSupervisionDocumentation { get; set; }
        public YesNoDontKnowChoice? RiskAssessmentConducted { get; set; }
        public DateTime? RiskAssessmentConductedDate { get; set; }
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        public SimpleLinkDTO RiskAssessmentDocumentation { get; set; }
        public string RiskAssessmentNotes { get; set; }
        public YesNoDontKnowChoice? DPIAConducted { get; set; }
        public DateTime? DPIADate { get; set; }
        public SimpleLinkDTO DPIADocumentation { get; set; }
        public YesNoDontKnowChoice? RetentionPeriodDefined { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        public int? DataRetentionEvaluationFrequencyInMonths { get; set; }

    }
}