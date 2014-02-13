using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public partial class Department
    {
        public Department()
        {
            this.Infrastructures = new List<Infrastructure>();
        }

        public int Id { get; set; }
        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
    }
}
