using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="ItInterface"/> 
    /// an <see cref="ItSystem"/> exhibts (udstiller).
    /// This is a (sys) 1:M (inf) relation.
    /// </summary>
    public class ItInterfaceExhibit : Entity
    {
        public ItInterfaceExhibit()
        {
            this.ItInterfaces = new List<ItInterface>();
            this.ItInterfaceExhibitUsage = new List<ItInterfaceExhibitUsage>();
        }

        public virtual ItSystem ItSystem { get; set; }

        public virtual ICollection<ItInterface> ItInterfaces { get; set; }

        public virtual ICollection<ItInterfaceExhibitUsage> ItInterfaceExhibitUsage { get; set; }
    }
}