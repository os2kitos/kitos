using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Shared;
using Presentation.Web.Models.API.V2.Types.SystemUsage;

namespace Presentation.Web.Models.API.V2.Request.SystemUsage
{
    public class GDPRWriteRequestDTO
    {
        /// <summary>Optional. No or null clears all deletion details. Populated details require IndividualRecords or BulkDeletion.</summary>
        [EnumDataType(typeof(PersonalDataDeletionCapabilityChoice))]
        public PersonalDataDeletionCapabilityChoice? PersonalDataDeletionCapability { get; set; }
        [EnumDataType(typeof(PersonalDataDeletionMethodChoice))]
        public PersonalDataDeletionMethodChoice? PersonalDataDeletionMethod { get; set; }
        /// <summary>
        /// Identifies a deadline choice available in the usage organization. An existing selection may be retained after it becomes unavailable.
        /// </summary>
        public Guid? PersonalDataDeletionDeadlineTypeUuid { get; set; }
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO? PersonalDataDeletionProcedureDocumentation { get; set; }
        /// <summary>Must identify an active member of the usage organization. Historical assignments are retained after departure or deletion.</summary>
        public Guid? PersonalDataDeletionResponsibleUserUuid { get; set; }
        public DateTime? PersonalDataDeletionVerifiedDate { get; set; }
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO? PersonalDataDeletionVerificationDocumentation { get; set; }
        public string? ProcessingPurpose { get; set; }
        /// <summary>
        /// Constraints: Name: 150 characters
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO? DirectoryDocumentation { get; set; }
        public IEnumerable<DataSensitivityLevelChoice>? DataSensitivityLevels { get; set; }
        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid>? SensitivePersonDataUuids { get; set; }
        /// <summary>
        /// Constraint: Can only be added if DataSensitivityLevelChoice.PersonData is present in DataSensitivityLevels field
        /// </summary>
        public IEnumerable<GDPRPersonalDataChoice>? SpecificPersonalData { get; set; }

        /// <summary>
        /// Constraint: If an update changes this field, the option identified must be currently available in the organization context
        /// </summary>
        public IEnumerable<Guid>? RegisteredDataCategoryUuids { get; set; }
        public YesNoDontKnowChoice? TechnicalPrecautionsInPlace { get; set; }
        public IEnumerable<TechnicalPrecautionChoice>? TechnicalPrecautionsApplied { get; set; }
        public SimpleLinkDTO? TechnicalPrecautionsDocumentation { get; set; }
        public YesNoDontKnowChoice? UserSupervision { get; set; }
        public DateTime? UserSupervisionDate { get; set; }
        public SimpleLinkDTO? UserSupervisionDocumentation { get; set; }
        public RiskAssessmentConductedChoice? RiskAssessmentConducted { get; set; }
        public DateTime? RiskAssessmentConductedDate { get; set; }
        [SupplierField]
        public RiskLevelChoice? RiskAssessmentResult { get; set; }
        public DateTime? PlannedRiskAssessmentDate { get; set; }
        /// <summary>
        /// Constraints:
        /// - Name: 150 characters
        /// </summary>
        [SimpleLinkNameMaxLength(150)]
        public SimpleLinkDTO? RiskAssessmentDocumentation { get; set; }
        public string? RiskAssessmentNotes { get; set; }
        public DpiaConductedChoice? DPIAConducted { get; set; }
        public DateTime? DPIADate { get; set; }
        public SimpleLinkDTO? DPIADocumentation { get; set; }
        public YesNoDontKnowChoice? RetentionPeriodDefined { get; set; }
        public DateTime? NextDataRetentionEvaluationDate { get; set; }
        public int? DataRetentionEvaluationFrequencyInMonths { get; set; }
        public IsDataProcessingAgreementRequiredChoice? IsDataProcessingAgreementRequired { get; set; }
    }
}
