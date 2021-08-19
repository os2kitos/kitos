using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class GDPRWriteRequestDTO
    {
        public string Purpose { get; set; }
        public YesNoDontKnowChoice? BusinessCritical { get; set; }
        public HostingChoice? HostedAt { get; set; }
        /// <summary>
        /// Constraints: Name: 150 characters
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO DirectoryDocumentation { get; set; }
        public IEnumerable<DataSensitivityLevelChoice> DataSensitivityLevels { get; set; }
        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid> SensitivePersonDataUuids { get; set; }
        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid> RegisteredDataCategorieUuids { get; set; }
        public YesNoDontKnowChoice? TechnicalPrecautionsInPlace { get; set; }
        public IEnumerable<TechnicalPrecautionChoice> TechnicalPrecautionsApplied { get; set; }
        public SimpleLinkDTO TechnicalPrecautionsDocumentation { get; set; }
        public YesNoDontKnowChoice? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public SimpleLinkDTO UserSupervisionDocumentation { get; set; }
        public YesNoDontKnowChoice? RiskAssessmentConducted { get; set; }
        public DateTime? RiskAssessmentConductedDate { get; set; }
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        /// <summary>
        /// Constraints:
        /// - Name: 150 characters
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
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