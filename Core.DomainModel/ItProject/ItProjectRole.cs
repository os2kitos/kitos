using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItProject
{
    class ItProjectRole : IOptionEntity<ItProject>
    {
        public ItProjectRole()
        {
            References = new List<ItProject>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<ItProject> References { get; set; }
    }
}
