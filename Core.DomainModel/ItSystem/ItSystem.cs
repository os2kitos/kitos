using System.Collections.Generic;

namespace Core.DomainModel.ItSystem
{
    public class ItSystem : IEntity<int>, IHasRights<ItSystemRight>, IHasAccessModifier
    {
        public ItSystem()
        {
            this.ExposedInterfaces = new List<ItSystem>();
            this.CanUseInterfaces = new List<ItSystem>();
            this.Children = new List<ItSystem>();
            
            this.Rights = new List<ItSystemRight>();
        }

        public int Id { get; set; }
        public int ParentId { get; set; }

        public bool IsPublic { get; set; }

        public int Version { get; set; } //TODO: this should probably be a reference to a version instead

        public int OrganizationId { get; set; }

        public bool IsInterface { get; set; }

        public virtual ICollection<ItSystem> ExposedBy { get; set; }
        public virtual ICollection<ItSystem> ExposedInterfaces { get; set; }

        public virtual ICollection<ItSystem> CanBeUsedBy { get; set; }
        public virtual ICollection<ItSystem> CanUseInterfaces { get; set; }

        public virtual ICollection<ItSystem> Children { get; set; }
        public virtual ItSystem Parent { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual ICollection<ItSystemRight> Rights { get; set; }

        public virtual AppType AppType { get; set; }
        public virtual UsageType UsageType { get; set; }

        public virtual ICollection<Wish> Wishes { get; set; }
    }
}
