using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class PaymentModel : IEntity<int>
    {
        public PaymentModel()
        {
            this.ItContracts = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Note { get; set; }

        public virtual ICollection<ItContract> ItContracts { get; set; }
    }
}
