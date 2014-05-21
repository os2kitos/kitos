using System;
using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class ItContract : IEntity<int>, IHasRights<ItContractRight>, IHasOwner
    {
        public ItContract()
        {
            //this.ShipNotices = new List<ShipNotice>();
            this.Rights = new List<ItContractRight>();
        }

        public int ObjectOwnerId { get; set; }
        public User ObjectOwner { get; set; }

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
        public OrganizationUnit ResponsibleOrganizationUnit { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }

        /// <summary>
        /// Id of the organization marked as supplier for this contract
        /// </summary>
        public int SupplierId { get; set; }
        public virtual Organization Supplier { get; set; }

        public virtual Agreement Agreement { get; set; } // TODO

        public int ProcurementStrategyId { get; set; }
        public ProcurementStrategy ProcurementStrategy { get; set; }

        public int ProcurementPlanId { get; set; }
        public virtual ProcurementPlan ProcurementPlan { get; set; }

        public int ContractTemplateId { get; set; }
        public virtual ContractTemplate ContractTemplate { get; set; }

        public int ContractTypeId { get; set; }
        public virtual ContractType ContractType { get; set; }

        public int PurchaseFormId { get; set; }
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
    }

    public class ProcurementStrategy : IOptionEntity<ItContract>
    {
        public ProcurementStrategy()
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

    public class ProcurementPlan
    {
        public int Half { get; set; }
        public int Year { get; set; }
    }
}
