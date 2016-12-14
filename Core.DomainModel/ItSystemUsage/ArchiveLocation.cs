using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystemUsage
{
    public class ArchiveLocation : OptionEntity<ItSystemUsage>, IOptionReference<ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage>();

    }
}
