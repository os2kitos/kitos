using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModel.ItSystem
{
    public class ItSystemUsageGDPR : Entity, IContextAware, ISystemModule
    {
        enum MyEnum
        {
            
        }

        public virtual ItSystemUsage.ItSystemUsage ItSystemUsage { get; set; }
        public bool IsInContext(int organizationId)
        {
            if (ItSystemUsage != null)
                return ItSystemUsage.IsInContext(organizationId);

            return false;
        }

        public override bool HasUserWriteAccess(User user)
        {
            if (ItSystemUsage != null && ItSystemUsage.HasUserWriteAccess(user))
                return true;

            return base.HasUserWriteAccess(user);
        }
    }
}
