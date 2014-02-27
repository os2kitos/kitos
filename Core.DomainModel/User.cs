using System.Collections.Generic;

namespace Core.DomainModel
{
    public class User : IEntity<int>
    {
        public User()
        {
            PasswordResetRequests = new List<PasswordResetRequest>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public int? Municipality_Id { get; set; }
        public int? Role_Id { get; set; }

        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; }
        public virtual Role Role { get; set; }
        public virtual Municipality Municipality { get; set; }
    }
}
