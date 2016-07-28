using System.Collections.Generic;

namespace Core.DomainModel.Organization
{
    public class OrganizationType
    {
        public OrganizationType()
        {
            this.Organizations = new HashSet<Organization>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public OrganizationCategory Category { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
    }
}
