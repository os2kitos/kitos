using System.Collections.Generic;

namespace Core.DomainModel.ItContract
{
    public class ContractTemplate
    {
        public ContractTemplate()
        {
            this.ItContracts = new List<ItContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<ItContract> ItContracts { get; set; }
    }
}
