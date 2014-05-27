using System;
using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItContract
{
    public class ItContract : IEntity<int>, IHasRights<ItContractRight>, IHasOwner
    {
        public ItContract()
        {
            //this.ShipNotices = new List<ShipNotice>();
            this.AgreementElements = new List<AgreementElement>();
            this.CustomAgreementElements = new List<CustomAgreementElement>();
            this.Children = new List<ItContract>();
            this.Rights = new List<ItContractRight>();
            this.AssociatedSystemUsages = new List<ItSystemUsage>();
            this.AssociatedInterfaceUsages = new List<InterfaceUsage>();
            this.AssociatedInterfaceExposures = new List<InterfaceExposure>();
            this.PaymentMilestones = new List<PaymentMilestone>();
        }

        public int ObjectOwnerId { get; set; }
        public virtual User ObjectOwner { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        public string ItContractId { get; set; }
        public string Esdh { get; set; }
        public string Folder { get; set; }
        public string SupplierContractSigner { get; set; }
        public bool HasSupplierSigned { get; set; }
        public DateTime? SupplierSignedDate { get; set; }
        public DateTime? OperationTestExpected { get; set; }
        public DateTime? OperationTestApproved { get; set; }
        public DateTime? OperationalAcceptanceTestExpected { get; set; }
        public DateTime? OperationalAcceptanceTestApproved { get; set; }
        public DateTime? Concluded { get; set; }
        public int Duration { get; set; }
        public DateTime? IrrevocableTo { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public DateTime? OperationRemunerationBegun { get; set; }
        public DateTime? Terminated { get; set; }
        public int ExtendMultiplier { get; set; }

        public int? TerminationDeadlineId { get; set; }
        public TerminationDeadline TerminationDeadline { get; set; }
        
        public ICollection<PaymentMilestone> PaymentMilestones { get; set; }

        public int? PaymentFreqencyId { get; set; }
        public PaymentFreqency PaymentFreqency { get; set; }

        public int? PaymentModelId { get; set; }
        public PaymentModel PaymentModel { get; set; }

        public int? PriceRegulationId { get; set; }
        public PriceRegulation PriceRegulation { get; set; }

        public int? OptionExtendId { get; set; }
        public OptionExtend OptionExtend { get; set; }

        public int? ContractSignerId { get; set; }
        public virtual User ContractSigner { get; set; }

        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// The chosen responsible org unit for this contract
        /// </summary>
        public int? ResponsibleOrganizationUnitId { get; set; }
        public virtual OrganizationUnit ResponsibleOrganizationUnit { get; set; }

        /// <summary>
        /// Id of the organization this contract was created under
        /// </summary>
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// Id of the organization marked as supplier for this contract
        /// </summary>
        public int? SupplierId { get; set; }
        public virtual Organization Supplier { get; set; }

        public int? ProcurementStrategyId { get; set; }
        public virtual ProcurementStrategy ProcurementStrategy { get; set; }

        public int? ProcurementPlanHalf { get; set; }
        public int? ProcurementPlanYear { get; set; }

        public int? ContractTemplateId { get; set; }
        public virtual ContractTemplate ContractTemplate { get; set; }

        public int? ContractTypeId { get; set; }
        public virtual ContractType ContractType { get; set; }

        public int? PurchaseFormId { get; set; }
        public virtual PurchaseForm PurchaseForm { get; set; }
        
        /// <summary>
        /// Id of parent ItContract
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// The parent ItContract
        /// </summary>
        public virtual ItContract Parent { get; set; }
        public virtual ICollection<ItContract> Children { get; set; }

        //public virtual ICollection<ShipNotice> ShipNotices { get; set; }
        public virtual ICollection<ItContractRight> Rights { get; set; }

        /// <summary>
        /// The (local usages of) it systems, that this contract is associated to. 
        /// </summary>
        public virtual ICollection<ItSystemUsage> AssociatedSystemUsages { get; set; }

        /// <summary>
        /// The interface usages that the contract is associated to. 
        /// </summary>
        public virtual ICollection<InterfaceUsage> AssociatedInterfaceUsages { get; set; } 
        public virtual ICollection<InterfaceExposure> AssociatedInterfaceExposures { get; set; } 

        public virtual ICollection<AgreementElement> AgreementElements { get; set; }
        public virtual ICollection<CustomAgreementElement> CustomAgreementElements { get; set; }
    }

    public class OptionExtend : IOptionEntity<ItContract>
    {
        public OptionExtend()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }

    public class PriceRegulation : IOptionEntity<ItContract>
    {
        public PriceRegulation()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }

    public class PaymentModel : IOptionEntity<ItContract>
    {
        public PaymentModel()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }

    public class PaymentFreqency : IOptionEntity<ItContract>
    {
        public PaymentFreqency()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }

    public class TerminationDeadline : IOptionEntity<ItContract>
    {
        public TerminationDeadline()
        {
            References = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItContract> References { get; set; }
    }

    public class PaymentMilestone : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? Expected { get; set; }
        public DateTime? Approved { get; set; }
        
        public int ItContractId { get; set; }
        public virtual ItContract ItContract { get; set; }
    }
}
