using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class Department : IEntity<int>
    {
        public Department()
        {
            this.Infrastructures = new List<Infrastructure>();
        }

        public int Id { get; set; }

        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
    }
}
