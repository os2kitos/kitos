using System.Collections.Generic;

namespace Core.DomainModel
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
        public virtual Municipality Municipality { get; set; }
    }
}
