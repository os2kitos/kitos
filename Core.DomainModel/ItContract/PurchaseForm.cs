using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class PurchaseForm
    {
        public PurchaseForm()
        {
            this.ItContracts = new List<ItContract>();
        }

        public int Id { get; set; }
        public virtual ICollection<ItContract> ItContracts { get; set; }
    }
}
