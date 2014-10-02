using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="ItInterface"/> 
    /// an <see cref="ItSystem"/> can use (kan anvende / anvender).
    /// This is an M:M relation.
    /// </summary>
    public class ItInterfaceUse
    {
        public ItInterfaceUse()
        {
            this.InterfaceUsages = new List<InterfaceUsage>();
        }

        public int ItInterfaceId { get; set; }
        public virtual ItInterface ItInterface { get; set; }

        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public virtual ICollection<InterfaceUsage> InterfaceUsages { get; set; }
    }
}