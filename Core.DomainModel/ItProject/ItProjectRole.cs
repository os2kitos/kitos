using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItProject
{
    public class ItProjectRole : IRoleEntity<ItProjectRight>
    {
        public ItProjectRole()
        {
            HasReadAccess = true;
            References = new List<ItProjectRight>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItProjectRight> References { get; set; }
    }
}
