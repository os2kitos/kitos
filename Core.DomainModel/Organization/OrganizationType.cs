using System.Collections.Generic;
// ReSharper disable VirtualMemberCallInConstructor

namespace Core.DomainModel.Organization
{
    /// <summary>
    /// 1 - Municipality
    /// 2 - Community of interest
    /// 3 - Company
    /// 4 - Other public authority
    /// </summary>
    public enum OrganizationTypeKeys
    {
        Kommune = 1,
        Interessefællesskab = 2,
        Virksomhed = 3,
        AndenOffentligMyndighed = 4
    }
    public class OrganizationType
    {
        public OrganizationType()
        {
            Organizations = new HashSet<Organization>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public OrganizationCategory Category { get; set; }
        public virtual ICollection<Organization> Organizations { get; set; }
    }
}
