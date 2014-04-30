using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class Supplier
    {
        public Supplier()
        {
            this.ItContracts = new List<ItContract.ItContract>();
        }

        public int Id { get; set; }
        public virtual ICollection<ItContract.ItContract> ItContracts { get; set; }
    }
}
