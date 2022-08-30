using System;
using System.Collections.Generic;
using Core.DomainModel.Shared;

namespace Core.DomainModel.ItContract.Read
{
    public class ItContractOverviewReadModel : IOwnedByOrganization, IReadModel<ItContract>, IHasName
    {
        public int OrganizationId { get; set; }
        public virtual Organization.Organization Organization { get; set; }
        public int Id { get; set; }
        public int SourceEntityId { get; set; }
        public virtual ItContract SourceEntity { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string ContractId { get; set; }
        public string ParentContractName { get; set; }

        public int? CriticalityId { get; set; }     // For filtering
        public string CriticalityName { get; set; } // For sorting

        public int? ResponsibleOrgUnitId { get; set; }     // For filtering
        public string ResponsibleOrgUnitName { get; set; } // For sorting
        public string SupplierName { get; set; }
        public string ContractSigner { get; set; }
        public int? ContractTypeId { get; set; }     // For filtering
        public string ContractTypeName { get; set; } // For sorting
        public int? ContractTemplateId { get; set; }     // For filtering
        public string ContractTemplateName { get; set; } // For sorting
        public int? PurchaseFormId { get; set; }     // For filtering
        public string PurchaseFormName { get; set; } // For sorting
        public int? ProcurementStrategyId { get; set; }     // For filtering
        public string ProcurementStrategyName { get; set; } // For sorting
        public int? ProcurementPlanYear { get; set; }
        public int? ProcurementPlanQuarter { get; set; }
        public YesNoUndecidedOption? ProcurementInitiated { get; set; }
        public ICollection<ItContractOverviewRoleAssignmentReadModel> RoleAssignments { get; set; }
        public ICollection<ItContractOverviewReadModelDataProcessingAgreement> DataProcessingAgreements { get; set; } //used for generating links and filtering IN collection (we can add index since the name can be constrained)
        public string DataProcessingAgreementsCsv { get; set; } //Used for sorting AND excel output (not filtering since we cannot set a ceiling on length and hence no index)
        public ICollection<ItContractOverviewReadModelItSystemUsage> ItSystemUsages { get; set; } //used for generating links and filtering IN collection (we can add index since the name can be constrained)
        public string ItSystemUsagesCsv { get; set; } //Used for sorting AND excel output 
        public string ItSystemUsagesSystemUuidCsv { get; set; } //Used for sorting AND excel output 
        public int NumberOfAssociatedSystemRelations { get; set; }
        public string ActiveReferenceTitle { get; set; }
        public string ActiveReferenceUrl { get; set; }
        public string ActiveReferenceExternalReferenceId { get; set; }
        public int? AccumulatedAcquisitionCost { get; set; }
        public int? AccumulatedOperationCost { get; set; }
        public int? AccumulatedOtherCost { get; set; }
        public DateTime? OperationRemunerationBegunDate { get; set; }
        public int? PaymentModelId { get; set; }     // For filtering
        public string PaymentModelName { get; set; } // For sorting
        public int? PaymentFrequencyId { get; set; }     // For filtering
        public string PaymentFrequencyName { get; set; } // For sorting
        public DateTime? LatestAuditDate { get; set; }
        public int? AuditStatusWhite { get; set; }
        public int? AuditStatusRed { get; set; }
        public int? AuditStatusYellow { get; set; }
        public int? AuditStatusGreen { get; set; }
        public int? AuditStatusMax { get; set; }
        public string Duration { get; set; }
        public int? OptionExtendId { get; set; }     // For filtering
        public string OptionExtendName { get; set; } // For sorting
        public int? TerminationDeadlineId { get; set; }     // For filtering
        public string TerminationDeadlineName { get; set; } // For sorting
        public DateTime? IrrevocableTo { get; set; }
        public DateTime? TerminatedAt { get; set; }
        public string LastEditedByUserName { get; set; }
        public DateTime? LastEditedAtDate { get; set; }
    }
}
