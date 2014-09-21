using System.Collections.Generic;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHasAccessModifier, IHierarchy<ItSystem>
    {
        public ItSystem()
        {
            this.ItInterfaceExhibits = new List<ItInterfaceExhibit>();
            this.CanUseInterfaces = new List<ItInterfaceUse>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.Wishes = new List<Wish>();
            this.Overviews = new List<ItSystemUsage.ItSystemUsage>();
            this.Usages = new List<ItSystemUsage.ItSystemUsage>();
            this.InfrastructureUsage = new List<InterfaceUsage>();
        }

        /// <summary>
        /// Gets or sets the user defined system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// The user defined system identifier.
        /// </value>
        public string ItSystemId { get; set; }

        public ItSystemType AppType { get; set; }

        public int? BelongsToId { get; set; }
        /// <summary>
        /// Gets or sets the organization the system belongs to.
        /// </summary>
        /// <remarks>
        /// Belongs to is a OIO term - think "produced by".
        /// </remarks>
        /// <value>
        /// The organization the it system belongs to.
        /// </value>
        public virtual Organization BelongsTo { get; set; }

        /// <summary>
        /// Gets or sets exhibited interfaces for this instance.
        /// </summary>
        /// <value>
        /// Exhibited interfaces.
        /// </value>
        public virtual ICollection<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }
        
        /// <summary>
        /// Gets or sets interfaces that can use this instance.
        /// </summary>
        /// <value>
        /// Usable interfaces.
        /// </value>
        public virtual ICollection<ItInterfaceUse> CanUseInterfaces { get; set; }
        
        /// <summary>
        /// Gets or sets the sub (child) it systems.
        /// </summary>
        /// <value>
        /// The children.
        /// </value>
        public virtual ICollection<ItSystem> Children { get; set; }
        
        public int? ParentId { get; set; }
        /// <summary>
        /// Gets or sets the parent (master) it system.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        public virtual ItSystem Parent { get; set; }
        
        public int? BusinessTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the business option.
        /// </summary>
        /// <value>
        /// The type of the business.
        /// </value>
        public virtual BusinessType BusinessType { get; set; }

        public virtual ICollection<Wish> Wishes { get; set; }

        public virtual ICollection<TaskRef> TaskRefs { get; set; }

        public virtual ICollection<ItSystemUsage.ItSystemUsage> Overviews { get; set; } // TODO what is this?

        /// <summary>
        /// Gets or sets local infrastructure usages of the system, in case the system is not an interface.
        /// </summary>
        /// <value>
        /// The infrastructure usage.
        /// </value>
        public virtual ICollection<InterfaceUsage> InfrastructureUsage { get; set; } // TODO is this used anywhere?

        /// <summary>
        /// Gets or sets the usages.
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> Usages { get; set; }
    }
}
