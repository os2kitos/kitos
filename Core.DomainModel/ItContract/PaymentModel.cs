using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public partial class PaymentModel
    {
        public PaymentModel()
        {
            this.ItContracts = new List<ItContract>();
        }

        public int Id { get; set; }
        public virtual ICollection<ItContract> ItContracts { get; set; }
    }
}
