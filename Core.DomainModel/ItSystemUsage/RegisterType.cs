using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class RegisterType : OptionHasChecked<ItSystemUsage.ItSystemUsage>, IOptionReference<ItSystemUsage.ItSystemUsage>
    {
        public virtual ICollection<ItSystemUsage.ItSystemUsage> References { get; set; } = new HashSet<ItSystemUsage.ItSystemUsage>();
    }
}
