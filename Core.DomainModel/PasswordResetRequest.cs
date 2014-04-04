using System;

namespace Core.DomainModel
{
    public class PasswordResetRequest : IEntity<string>
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}