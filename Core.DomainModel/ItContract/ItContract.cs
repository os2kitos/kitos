using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel.ItContract
{
    public class ItContract : IEntity<int>, IHasRights<ItContractRight>
    {
        public ItContract()
        {
            this.ShipNotices = new List<ShipNotice>();
            this.Rights = new List<ItContractRight>();
        }

        public int Id { get; set; }
        public int ContractTypeId { get; set; }
        public int ContractTemplateId { get; set; }
        public int PurchaseFormId { get; set; }
        public int PaymentModelId { get; set; }
        public int SupplierId { get; set; }
        public int MunicipalityId { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual ContractTemplate ContractTemplate { get; set; }
        public virtual ContractType ContractType { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual PaymentModel PaymentModel { get; set; }
        public virtual PurchaseForm PurchaseForm { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual ICollection<ShipNotice> ShipNotices { get; set; }
        public virtual ICollection<ItContractRight> Rights { get; set; }

        /// <summary>
        /// The (local usages of) it systems, that this contract is associated to. 
        /// </summary>
        public virtual ICollection<ItSystemUsage> AssociatedSystems { get; set; }

        /// <summary>
        /// The interface usages that the contract is associated to. 
        /// A many-to-one relation - every InterfaceUsage must have a contract.
        /// </summary>
        public virtual ICollection<InterfaceUsage> AssociatedInterfaces { get; set; } 
    }
}
