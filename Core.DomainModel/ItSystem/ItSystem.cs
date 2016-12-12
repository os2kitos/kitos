using System.Collections.Generic;
using Core.DomainModel.Organization;

namespace Core.DomainModel.ItSystem
{
    using System;

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
            this.AccessTypes = new List<AccessType>();
            this.Wishes = new List<Wish>();
            this.Usages = new List<ItSystemUsage.ItSystemUsage>();
        }

        public bool IsActive
        {
            get
            {
                if (!this.Active)
                {
                    var today = DateTime.UtcNow;
                    var startDate = this.Concluded ?? today;
                    var endDate = DateTime.MaxValue;

                    if (ExpirationDate.HasValue && ExpirationDate.Value != DateTime.MaxValue)
                    {
                        endDate = ExpirationDate.Value.Date.AddDays(1).AddTicks(-1);
                    }

                    if (this.Terminated.HasValue)
                    {
                        var terminationDate = this.Terminated;
                        if (this.TerminationDeadlineInSystem != null)
                        {
                            int deadline;
                            int.TryParse(this.TerminationDeadlineInSystem.Name, out deadline);
                            terminationDate = this.Terminated.Value.AddMonths(deadline);
                        }
                        // indgået-dato <= dags dato <= opsagt-dato + opsigelsesfrist
                        return today >= startDate.Date && today <= terminationDate.Value.Date.AddDays(1).AddTicks(-1);
                    }

                    // indgået-dato <= dags dato <= udløbs-dato
                    return today >= startDate.Date && today <= endDate;
                }
                return this.Active;
            }
        }

        /// <summary>
        ///     Gets or sets Active.
        /// </summary>
        /// <value>
        ///   Active.
        /// </value>
        public bool Active { get; set; }

        /// <summary>
        ///     When the system began. (indgået)
        /// </summary>
        /// <value>
        ///     The concluded date.
        /// </value>
        public DateTime? Concluded { get; set; }

        /// <summary>
        ///     When the system expires. (udløbet)
        /// </summary>
        /// <value>
        ///     The expiration date.
        /// </value>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        ///     Date the system ends. (opsagt)
        /// </summary>
        /// <value>
        ///     The termination date.
        /// </value>
        public DateTime? Terminated { get; set; }

        //public int? TerminationDeadlineId { get; set; }

        /// <summary>
        ///     Gets or sets the termination deadline option. (opsigelsesfrist)
        /// </summary>
        /// <remarks>
        ///     Added months to the <see cref="Terminated" /> contract termination date before the contract expires.
        ///     It's a string but should be treated as an int.
        ///     TODO perhaps a redesign of OptionEntity is in order
        /// </remarks>
        /// <value>
        ///     The termination deadline.
        /// </value>
        public virtual TerminationDeadlineTypesInSystem TerminationDeadlineInSystem { get; set; }

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

        public virtual ICollection<AccessType> AccessTypes { get; set; }

        /// <summary>
        /// Gets or sets the usages.
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage.ItSystemUsage> Usages { get; set; }

        public class TerminationDeadlineTypesInSystem : OptionEntity<ItSystem>, IOptionReference<ItSystem>
        {
            public virtual ICollection<ItSystem> References { get; set; } = new HashSet<ItSystem>();
        }
    }
}
