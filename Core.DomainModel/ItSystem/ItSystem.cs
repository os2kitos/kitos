using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    /// <summary>
    /// Represents an it system.
    /// </summary>
    public class ItSystem : Entity, IHasAccessModifier, IHierarchy<ItSystem>
    {
        public ItSystem()
        {
            this.ExposedInterfaces = new List<ItSystem>();
            this.CanUseInterfaces = new List<ItSystem>();
            this.CanBeUsedBy = new List<ItSystem>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.Usages = new List<ItSystemUsage>();
            this.Wishes = new List<Wish>();
            this.TaskRefs = new List<TaskRef>();
            this.Overviews = new List<ItSystemUsage>();
            this.DataRows = new List<DataRow>();
            this.InterfaceLocalUsages = new List<InterfaceUsage>();
            this.InterfaceLocalExposure = new List<InterfaceExposure>();
            this.InfrastructureUsage = new List<InterfaceUsage>();
        }
        
        public string Version { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the user defined system identifier.
        /// </summary>
        /// <remarks>
        /// This identifier is NOT the primary key.
        /// </remarks>
        /// <value>
        /// The user defined system identifier.
        /// </value>
        public string SystemId { get; set; }
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
        public AccessModifier AccessModifier { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int? ExposedById { get; set; }
        /// <summary>
        /// Gets or sets it system that exposes this interface instance.
        /// </summary>
        /// <remarks>
        /// Should only be set/used if this instance's <see cref="AppType"/> is an interface.
        /// </remarks>
        /// <value>
        /// The it system that exposes this instance.
        /// </value>
        public virtual ItSystem ExposedBy { get; set; }
        /// <summary>
        /// Gets or sets interfaces this instance exposes.
        /// </summary>
        /// <value>
        /// Exposed interfaces.
        /// </value>
        public virtual ICollection<ItSystem> ExposedInterfaces { get; set; }
        /// <summary>
        /// Gets or sets it systems that can use this instance.
        /// </summary>
        /// <remarks>
        /// Should only be set/used if this instance's <see cref="AppType"/> is an interface.
        /// </remarks>
        /// <value>
        /// It systems that can used by this instance.
        /// </value>
        public virtual ICollection<ItSystem> CanBeUsedBy { get; set; }
        /// <summary>
        /// Gets or sets interfaces that can use this instance.
        /// </summary>
        /// <remarks>
        /// Should only be set/used if this instance's <see cref="AppType"/> is not an interface.
        /// </remarks>
        /// <value>
        /// Usable interfaces.
        /// </value>
        public virtual ICollection<ItSystem> CanUseInterfaces { get; set; }
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
        public int OrganizationId { get; set; }
        /// <summary>
        /// Gets or sets the organization this instance was created under.
        /// </summary>
        /// <value>
        /// The organization.
        /// </value>
        public virtual Organization Organization { get; set; }
        /// <summary>
        /// Gets or sets the usages (binding between system and org).
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage> Usages { get; set; }

        public int? AppTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the application.
        /// </summary>
        /// <remarks>
        /// This is an important property as it changes what properties are relevant.
        /// </remarks>
        /// <value>
        /// The type of the application.
        /// </value>
        public virtual AppType AppType { get; set; }

        public int? BusinessTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the business option.
        /// </summary>
        /// <value>
        /// The type of the business.
        /// </value>
        public virtual BusinessType BusinessType { get; set; }

        #region Interface "Snitflade" data

        public int? InterfaceId { get; set; }
        /// <summary>
        /// Gets or sets the interface option.
        /// Provides details about an it system of type interface.
        /// </summary>
        /// <value>
        /// The interface option.
        /// </value>
        public virtual Interface Interface { get; set; }

        public int? InterfaceTypeId { get; set; }
        /// <summary>
        /// Gets or sets the type of the interface.
        /// Provides details about an it system of type interface.
        /// </summary>
        /// <value>
        /// The type of the interface.
        /// </value>
        public virtual InterfaceType InterfaceType { get; set; }

        public int? TsaId { get; set; }
        public virtual Tsa Tsa { get; set; }

        public int? MethodId { get; set; }
        public virtual Method Method { get; set; }

        public virtual ICollection<DataRow> DataRows { get; set; } 

        #endregion

        public virtual ICollection<Wish> Wishes { get; set; }

        public virtual ICollection<TaskRef> TaskRefs { get; set; }

        public virtual ICollection<ItSystemUsage> Overviews { get; set; }

        /// <summary>
        /// Gets or sets local usages of the system, in case the system is an interface.
        /// </summary>
        /// <value>
        /// The interface local usages.
        /// </value>
        public virtual ICollection<InterfaceUsage> InterfaceLocalUsages { get; set; }

        /// <summary>
        /// Gets or sets local exposure of the system, in case the system is an interface.
        /// </summary>
        /// <value>
        /// The interface local exposure.
        /// </value>
        public virtual ICollection<InterfaceExposure> InterfaceLocalExposure { get; set; }

        /// <summary>
        /// Gets or sets local infrastructure usages of the system, in case the system is not an interface.
        /// </summary>
        /// <value>
        /// The infrastructure usage.
        /// </value>
        public virtual ICollection<InterfaceUsage> InfrastructureUsage { get; set; }
    }
}
