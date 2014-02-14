using System.Collections.Generic;

namespace Core.DomainModel
{
    public class User
    {
        public User()
        {
            Roles = new List<Role>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public virtual List<Role> Roles { get; set; }
    }
}