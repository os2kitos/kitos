using System.Collections.Generic;

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
        public int ContractType_Id { get; set; }
        public int ContractTemplate_Id { get; set; }
        public int PurchaseForm_Id { get; set; }
        public int PaymentModel_Id { get; set; }
        public int Supplier_Id { get; set; }
        public int Municipality_Id { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual ContractTemplate ContractTemplate { get; set; }
        public virtual ContractType ContractType { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual PaymentModel PaymentModel { get; set; }
        public virtual PurchaseForm PurchaseForm { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual Municipality Municipality { get; set; }
        public virtual ICollection<ShipNotice> ShipNotices { get; set; }
        public virtual ICollection<ItContractRight> Rights { get; set; }
    }
}
