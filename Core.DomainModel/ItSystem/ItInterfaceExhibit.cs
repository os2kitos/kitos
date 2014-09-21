using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents what <see cref="DomainModel.ItSystem.ItInterface"/> 
    /// an <see cref="ItSystem"/> exhibts (udstiller).
    /// This is a (sys) 1:M (inf) relation.
    /// </summary>
    public class ItInterfaceExhibit : Entity
    {
        public ItInterfaceExhibit()
        {
            this.ItInterfaceExhibitUsage = new List<ItInterfaceExhibitUsage>();
        }

        public int ItSystemId { get; set; }
        public virtual ItSystem ItSystem { get; set; }

        public virtual ItInterface ItInterface { get; set; }

        public virtual ICollection<ItInterfaceExhibitUsage> ItInterfaceExhibitUsage { get; set; }
        
    }
}