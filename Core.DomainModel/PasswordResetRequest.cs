using System;

namespace Core.DomainModel
{
    public class PasswordResetRequest : Entity
    {
        public string Hash { get; set; }

        public DateTime Time { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}