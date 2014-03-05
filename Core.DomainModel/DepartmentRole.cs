using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class DepartmentRole : IOptionEntity<DepartmentRight>
    {
        public DepartmentRole()
        {
            References = new List<DepartmentRight>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<DepartmentRight> References { get; set; }
    }
}
