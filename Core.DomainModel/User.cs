using System.Collections.Generic;

namespace Core.DomainModel
{
    public class User : IEntity<int>
    {
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
            Roles = new List<Role>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        public virtual Municipality Municipality { get; set; }
    }
}
