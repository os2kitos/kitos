using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class Department : IEntity<int>, IHasRights<DepartmentRight>
    {
        public Department()
        {
            this.Infrastructures = new List<Infrastructure>();
            this.Rights = new List<DepartmentRight>();
        }

        public int Id { get; set; }

        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
        public virtual ICollection<DepartmentRight> Rights { get; set; }
    }
}
