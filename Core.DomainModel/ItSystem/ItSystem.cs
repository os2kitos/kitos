using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHasAccessModifier, IHierarchy<ItSystem>
    {
        public ItSystem()
        {
            this.ExposedInterfaces = new List<ItInterface>();
            this.CanUseInterfaces = new List<ItInterface>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.Usages = new List<ItSystemUsage>();
            this.Wishes = new List<Wish>();
            this.TaskRefs = new List<TaskRef>();
            this.Overviews = new List<ItSystemUsage>();
            this.InfrastructureUsage = new List<InterfaceUsage>();
        }
        
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
        /// Gets or sets interfaces this instance exposes.
        /// </summary>
        /// <value>
        /// Exposed interfaces.
        /// </value>
        public virtual ICollection<ItInterface> ExposedInterfaces { get; set; }
        
        /// <summary>
        /// Gets or sets interfaces that can use this instance.
        /// </summary>
        /// <remarks>
        /// Should only be set/used if this instance's <see cref="AppType"/> is not an interface.
        /// </remarks>
        /// <value>
        /// Usable interfaces.
        /// </value>
        public virtual ICollection<ItInterface> CanUseInterfaces { get; set; }
        
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

        public virtual ICollection<ItSystemUsage> Overviews { get; set; }

        /// <summary>
        /// Gets or sets local infrastructure usages of the system, in case the system is not an interface.
        /// </summary>
        /// <value>
        /// The infrastructure usage.
        /// </value>
        public virtual ICollection<InterfaceUsage> InfrastructureUsage { get; set; }
    }
}
