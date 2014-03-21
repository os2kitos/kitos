using System.Collections.Generic;

namespace Core.DomainModel
{
    public class AdminRole : IOptionEntity<AdminRight>
    {
        public AdminRole()
        {
            References = new List<AdminRight>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuggestion { get; set; }
        public string Note { get; set; }
        public virtual ICollection<AdminRight> References { get; set; }
    }
}