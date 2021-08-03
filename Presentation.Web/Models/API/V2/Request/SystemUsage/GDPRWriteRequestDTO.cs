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
        public YesNoExtendedChoice? BusinessCritical { get; set; }
        public HostingChoice? HostedAt { get; set; }
        /// <summary>
        /// Constraints: Name: 150 characters
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO DirectoryDocumentation { get; set; }
        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<DataSensitivityLevelChoice> DataSensitivityLevels { get; set; }
        /// <summary>
        /// Constraint: DataSensitivityLevels must contain PersonData
        /// </summary>
        public IEnumerable<Guid> SensitivePersonDataUuids { get; set; }
        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid> RegisteredDataCategorieUuids { get; set; }
        public YesNoExtendedChoice? TechnicalPrecautionsInPlace { get; set; }
        /// <summary>
        /// Constraint: TechnicalPrecautionsInPlace must be set to Yes
        /// </summary>
        public IEnumerable<TechnicalPrecautionChoice> TechnicalPrecautionsApplied { get; set; }
        /// <summary>
        /// Constraint: TechnicalPrecautionsInPlace must be set to Yes
        /// </summary>
        public SimpleLinkDTO TechnicalPrecautionsDocumentation { get; set; }
        public YesNoExtendedChoice? UserSupervision { get; set; }
        /// <summary>
        /// Constraint: UserSupervision must be set to Yes
        /// </summary>
        public DateTime? UserSupervisionDate { get; set; }
        /// <summary>
        /// Constraint: UserSupervision must be set to Yes
        /// </summary>
        public SimpleLinkDTO UserSupervisionDocumentation { get; set; }
        public YesNoExtendedChoice? RiskAssessmentConducted { get; set; }
        /// <summary>
        /// Constraint: RiskAssessmentConducted must be set to Yes
        /// </summary>
        public DateTime? RiskAssessmentConductedDate { get; set; }
        /// <summary>
        /// Constraint: RiskAssessmentConducted must be set to Yes
        /// </summary>
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        /// <summary>
        /// Constraints:
        /// - Name: 150 characters
        /// - RiskAssessmentConducted must be set to Yes
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO RiskAssessmentDocumentation { get; set; }
        /// <summary>
        /// Constraint: RiskAssessmentConducted must be set to Yes
        /// </summary>
        public string RiskAssessmentNotes { get; set; }
        public YesNoExtendedChoice? DPIAConducted { get; set; }
        /// <summary>
        /// Constraint: DPIAConducted must be set to Yes
        /// </summary>
        public DateTime? DPIADate { get; set; }
        /// <summary>
        /// Constraint: DPIAConducted must be set to Yes
        /// </summary>
        public SimpleLinkDTO DPIADocumentation { get; set; }
        public YesNoExtendedChoice? RetentionPeriodDefined { get; set; }
        /// <summary>
        /// Constraint: NextDataRetentionEvaluationDate must be set to Yes
        /// </summary>
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        /// <summary>
        /// Constraint: NextDataRetentionEvaluationDate must be set to Yes
        /// </summary>
        public int? DataRetentionEvaluationFrequencyInMonths { get; set; }
    }
}