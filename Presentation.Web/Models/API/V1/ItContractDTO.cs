﻿using System;
using System.Collections.Generic;
using Core.DomainModel.ItContract;
using Core.DomainModel.Shared;

namespace Presentation.Web.Models.API.V1
{
    public class ItContractDTO
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string ItContractId { get; set; }
        public string SupplierContractSigner { get; set; }
        public bool HasSupplierSigned { get; set; }
        public DateTime? SupplierSignedDate { get; set; }
        public DateTime? Concluded { get; set; }
        public int? DurationYears { get; set; }
        public int? DurationMonths { get; set; }
        public bool DurationOngoing { get; set; }
        public DateTime? IrrevocableTo { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? OperationRemunerationBegun { get; set; }
        public DateTime? Terminated { get; set; }
        public int ExtendMultiplier { get; set; }

        public int? TerminationDeadlineId { get; set; }
        public string TerminationDeadlineName { get; set; }
        public int? PaymentFreqencyId { get; set; }
        public string PaymentFreqencyName { get; set; }
        public int? PaymentModelId { get; set; }
        public string PaymentModelName { get; set; }
        public int? PriceRegulationId { get; set; }
        public string PriceRegulationName { get; set; }
        public int? OptionExtendId { get; set; }
        public string OptionExtendName { get; set; }
        public string ContractSigner { get; set; }

        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public int? ResponsibleOrganizationUnitId { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public Guid? SupplierUuid { get; set; }
        public int? ProcurementStrategyId { get; set; }
        public string ProcurementStrategyName { get; set; }
        public int? ProcurementPlanQuarter { get; set; }
        public int? ProcurementPlanYear { get; set; }
        public int? ContractTemplateId { get; set; }
        public string ContractTemplateName { get; set; }
        public int? ContractTypeId { get; set; }
        public string ContractTypeName { get; set; }
        public Guid? ContractTypeUuid { get; set; }
        public int? PurchaseFormId { get; set; }
        public string PurchaseFormName { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public IEnumerable<OptionDTO> AgreementElements { get; set; }

        public IEnumerable<ItSystemUsageSimpleDTO> AssociatedSystemUsages { get; set; }

        public IEnumerable<AdviceDTO> Advices { get; set; }
        public DateTime LastChanged { get; set; }
        public string LastChangedByName { get; set; }
        public int LastChangedByUserId { get; set; }
        
        public string ObjectOwnerFullName { get; set; }

        public int? ObjectOwnerId { get; set; }

        public YearSegmentOption? Running { get; set; }
        public YearSegmentOption? ByEnding { get; set; }
        public bool? Active { get; set; }
        public bool? IsActive { get; set; }
        public ICollection<ExternalReferenceDTO> ExternalReferences { get; set; }
        public int? ReferenceId { get; set; }
        public ExternalReferenceDTO Reference;

        public IEnumerable<NamedEntityDTO> DataProcessingRegistrations { get; set; }

        public Guid Uuid { get; set; }
        public int? CriticalityId { get; set; }
        public string CriticalityName { get; set; }

        public YesNoUndecidedOption? ProcurementInitiated { get; set; }
    }
}
