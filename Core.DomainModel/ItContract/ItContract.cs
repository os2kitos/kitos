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
            this.AssociatedSystems = new List<ItSystemUsage>();
            this.AssociatedInterfaceUsages = new List<InterfaceUsage>();
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
        public User ContractSigner { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }

        public int? ResponsibleOrganizationUnitId { get; set; }
        public virtual OrganizationUnit ResponsibleOrganizationUnit { get; set; }

        /// <summary>
        /// Id of the organization marked as supplier for this contract
        /// </summary>
        public int? SupplierId { get; set; }
        public virtual Organization Supplier { get; set; }

        public int? ProcurementStrategyId { get; set; }
        public ProcurementStrategy ProcurementStrategy { get; set; }

        public int? ProcurementPlanId { get; set; }
        public virtual ProcurementPlan ProcurementPlan { get; set; }

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
        public virtual ICollection<ItSystemUsage> AssociatedSystems { get; set; }

        /// <summary>
        /// The interface usages that the contract is associated to. 
        /// </summary>
        public virtual ICollection<InterfaceUsage> AssociatedInterfaceUsages { get; set; } 

        public virtual ICollection<InterfaceExposure> AssociatedInterfaceExposures { get; set; } 
        public virtual ICollection<AgreementElement> AgreementElements { get; set; }
        public virtual ICollection<CustomAgreementElement> CustomAgreementElements { get; set; }
    }
}
