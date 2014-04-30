using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class InterfaceCategory : IOptionEntity<InterfaceUsage>
    {
        public InterfaceCategory()
        {
            this.References = new List<InterfaceUsage>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<InterfaceUsage> References { get; set; }
    }
}
