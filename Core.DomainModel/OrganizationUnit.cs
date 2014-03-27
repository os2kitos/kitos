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
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? Parent_Id { get; set; }
        public int Organization_Id { get; set; }

        public virtual OrganizationUnit Parent { get; set; }
        public virtual ICollection<OrganizationUnit> Children { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual ICollection<Infrastructure> Infrastructures { get; set; }
        public virtual ICollection<OrganizationRight> Rights { get; set; }
    }
}
