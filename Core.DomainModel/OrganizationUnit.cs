using System.Collections.Generic;
using Core.DomainModel.ItSystem;

namespace Core.DomainModel
{
    public class OrganizationUnit : IEntity<int>, IHasRights<OrganizationRight>
    {
        public OrganizationUnit()
        {
            this.Infrastructures = new List<Infrastructure>();
            this.Rights = new List<OrganizationRight>();
            this.TaskRefs = new List<TaskRef>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public int OrganizationId { get; set; }

        public virtual OrganizationUnit Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
        public virtual ICollection<OrganizationRight> Rights { get; set; }
        public virtual ICollection<TaskRef> TaskRefs { get; set; }
        public virtual ICollection<TaskRef> OwnedTasks { get; set; }
    }
}
