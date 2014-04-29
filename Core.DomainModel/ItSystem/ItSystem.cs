using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Core.DomainModel.ItSystem
{
    public class ItSystem : IEntity<int>, IHasRights<ItSystemRight>, IHasAccessModifier
    {
        public ItSystem()
        {
            this.ExposedInterfaces = new List<ItSystem>();
            this.CanUseInterfaces = new List<ItSystem>();
            this.CanBeUsedBy = new List<ItSystem>();
            this.Children = new List<ItSystem>();
            this.TaskRefs = new List<TaskRef>();
            this.Rights = new List<ItSystemRight>();
            this.Usages = new List<ItSystemUsage>();
            this.Wishes = new List<Wish>();
            this.TaskRefs = new List<TaskRef>();
            this.Overviews = new List<ItSystemUsage>();
            this.DataRows = new List<DataRow>();
            this.InterfaceUsages = new List<InterfaceUsage>();
        }

        public int Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string SystemId { get; set; }

        public int? ExposedById { get; set; }

        public int BelongsToId { get; set; }
        /// <summary>
        /// The organization the system belongs to (OIO term - think "produced by")
        /// </summary>
        public virtual Organization BelongsTo { get; set; }

        public int OrganizationId { get; set; }
        public int UserId { get; set; }


        public AccessModifier AccessModifier { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// If this system is an interface, which system exposed it
        /// </summary>
        public virtual ItSystem ExposedBy { get; set; }
        
        /// <summary>
        /// Which interfaces does this system expose
        /// </summary>
        public virtual ICollection<ItSystem> ExposedInterfaces { get; set; }

        /// <summary>
        /// If this system is an interface, which system can use it?
        /// </summary>
        public virtual ICollection<ItSystem> CanBeUsedBy { get; set; }
        
        /// <summary>
        /// Which interfaces can this system use
        /// </summary>
        public virtual ICollection<ItSystem> CanUseInterfaces { get; set; }

        /// <summary>
        /// Sub system
        /// </summary>
        public virtual ICollection<ItSystem> Children { get; set; }
        
        public int? ParentId { get; set; }
        /// <summary>
        /// Super systems
        /// </summary>
        public virtual ItSystem Parent { get; set; }

        /// <summary>
        /// which organization the it system was created under
        /// </summary>
        public virtual Organization Organization { get; set; }
        
        /// <summary>
        /// Created by
        /// </summary>
        public virtual User User { get; set; }

        public virtual ICollection<ItSystemRight> Rights { get; set; }

        /// <summary>
        /// Usages (binding between system and org)
        /// </summary>
        /// <value>
        /// The usages.
        /// </value>
        public virtual ICollection<ItSystemUsage> Usages { get; set; }

        public int AppTypeId { get; set; }
        public virtual AppType AppType { get; set; }

        public int BusinessTypeId { get; set; }
        public virtual BusinessType BusinessType { get; set; }

        #region Interface "Snitflade" data
        public int? InterfaceId { get; set; }
        public virtual Interface Interface { get; set; }

        public int? InterfaceTypeId { get; set; }
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
        /// Local usages of the system, in case the system is an interface
        /// </summary>
        public virtual ICollection<InterfaceUsage> InterfaceUsages { get; set; }
    }
}
