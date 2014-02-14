using System;

namespace Core.DomainModel
{
    public class PasswordResetRequest
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public DateTime Time { get; set; }
        public User User { get; set; }
    }
}