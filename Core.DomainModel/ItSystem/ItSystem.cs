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
        public string Version { get; set; }
        public string Name { get; set; }

        public int? ParentId { get; set; }
        public int? ExposedById { get; set; }

        public int OrganizationId { get; set; }
        public int UserId { get; set; }

        public AccessModifier AccessModifier { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }

        public virtual ItSystem ExposedBy { get; set; }
        public virtual ICollection<ItSystem> ExposedInterfaces { get; set; }

        public virtual ICollection<ItSystem> CanBeUsedBy { get; set; }
        public virtual ICollection<ItSystem> CanUseInterfaces { get; set; }

        public virtual ICollection<ItSystem> Children { get; set; }
        public virtual ItSystem Parent { get; set; }

        public virtual Organization Organization { get; set; } //created inside which organization?
        public virtual User User { get; set; } //created by

        public virtual ICollection<ItSystemRight> Rights { get; set; }

        public virtual AppType AppType { get; set; }
        public virtual BusinessType BusinessType { get; set; }

        public virtual ICollection<Wish> Wishes { get; set; }



        //TODO KLE!!
    }
}
