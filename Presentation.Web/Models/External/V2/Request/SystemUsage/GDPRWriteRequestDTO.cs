using System;
using System.Collections.Generic;
using Presentation.Web.Models.External.V2.Types.Shared;
using Presentation.Web.Models.External.V2.Types.SystemUsage;

namespace Presentation.Web.Models.External.V2.Request.SystemUsage
{
    public class GDPRWriteRequestDTO
    {
        public string Purpose { get; set; }
        public YesNoExtendedChoice? BusinessCritical { get; set; }
        public HostingChoice? HostedAt { get; set; }
        public SimpleLinkDTO DirectoryDocumentation { get; set; }
        public IEnumerable<DataSensitivityLevelChoice> DataSensitivityLevels { get; set; }
        public IEnumerable<Guid> SensitivePersonDataUuids { get; set; }
        public IEnumerable<Guid> RegisteredDataCategorieUuids { get; set; }
        public YesNoExtendedChoice? TechnicalPrecautionsInPlace { get; set; }
        public IEnumerable<TechnicalPrecautionChoice> TechnicalPrecautionsApplied { get; set; }
        public SimpleLinkDTO TechnicalPrecautionsDocumentation { get; set; }
        public YesNoExtendedChoice? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public SimpleLinkDTO UserSupervisionDocumentation { get; set; }
        public YesNoExtendedChoice? RiskAssessmentConducted { get; set; }
        public DateTime? RiskAssessmentConductedDate { get; set; }
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        public SimpleLinkDTO RiskAssessmentDocumentation { get; set; }
        public string RiskAssessmentNotes { get; set; }
        public YesNoExtendedChoice? DPIAConducted { get; set; }
        public DateTime? DPIADate { get; set; }
        public SimpleLinkDTO DPIADocumentation { get; set; }
        public YesNoExtendedChoice? RetentionPeriodDefined { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        public int? DataRetentionEvaluationFrequencyInMonths { get; set; }
    }
}