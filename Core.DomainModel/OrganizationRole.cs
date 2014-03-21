using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel
{
    public class OrganizationRole : IOptionEntity<OrganizationRight>
    {
        public OrganizationRole()
        {
            References = new List<OrganizationRight>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<OrganizationRight> References { get; set; }
    }
}
