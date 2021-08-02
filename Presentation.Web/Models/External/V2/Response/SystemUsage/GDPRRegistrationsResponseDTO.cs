using System;
using System.Collections.Generic;
using Presentation.Web.Models.External.V2.Types.Shared;

namespace Presentation.Web.Models.External.V2.Response.SystemUsage
{
    public class GDPRRegistrationsResponseDTO
    {
        public string Purpose { get; set; }
        public YesNoExtendedChoice? BusinessCritical { get; set; }
        public HostingChoice? HostedAt { get; set; }
        public SimpleLinkResponseDTO DirectoryDocumentation { get; set; }
        public IEnumerable<DataSensitivityLevelChoice> DataSensitivityLevels { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> SensitivePersonData { get; set; }
        public IEnumerable<IdentityNamePairResponseDTO> RegisteredDataCategories { get; set; }
        public YesNoExtendedChoice? TechnicalPrecautionsInPlace { get; set; }
        public IEnumerable<TechnicalPrecautionChoice> TechnicalPrecautionsApplied { get; set; }
        public SimpleLinkResponseDTO TechnicalPrecautionsDocumentation { get; set; }
        public YesNoExtendedChoice? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public SimpleLinkResponseDTO UserSupervisionDocumentation { get; set; }
        public YesNoExtendedChoice? RiskAssessmentConducted { get; set; }
        public DateTime? RiskAssessmentConductedDate { get; set; }
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        public SimpleLinkResponseDTO RiskAssessmentDocumentation { get; set; }
        public string RiskAssessmentNotes { get; set; }
        public YesNoExtendedChoice? DPIAConducted { get; set; }
        public DateTime? DPIADate { get; set; }
        public SimpleLinkResponseDTO DPIADocumentation { get; set; }
        public YesNoExtendedChoice? RetentionPeriodDefined { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        public int? DataRetentionEvaluationFrequencyInMonths { get; set; }

    }
}