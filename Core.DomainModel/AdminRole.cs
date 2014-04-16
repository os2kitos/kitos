using System.Collections.Generic;

namespace Core.DomainModel
{
    public class AdminRole : IRoleEntity<AdminRight>
    {
        public AdminRole()
        {
            References = new List<AdminRight>();
            HasReadAccess = true;
            HasWriteAccess = true;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AdminRight> References { get; set; }
        public bool HasReadAccess { get; set; }
        public bool HasWriteAccess { get; set; }
    }
}