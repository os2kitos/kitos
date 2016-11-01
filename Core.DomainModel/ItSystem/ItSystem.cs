using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : ItSystemBase, IHasAccessModifier, IHierarchy<ItSystem>, IHasReferences
    {
        public ItSystem()
        {
            this.ItInterfaceExhibits = new List<ItInterfaceExhibit>();
            this.CanUseInterfaces = new List<ItInterfaceUse>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.Wishes = new List<Wish>();
            this.Usages = new List<ItSystemUsage.ItSystemUsage>();
            ExternalReferences = new List<ExternalReference>();
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

        public int? AppTypeOptionId { get; set; }
        public virtual ItSystemType AppTypeOption { get; set; }

        /// <summary>
        /// Gets or sets exhibited interfaces for this instance.
        /// </summary>
        /// <value>
        /// Exhibited interfaces.
        /// </value>
        public virtual ICollection<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }

        public virtual ICollection<ExternalReference> ExternalReferences { get; set; }

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

        /// <summary>
        /// Gets or sets the usages.
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> Usages { get; set; }
    }
}
