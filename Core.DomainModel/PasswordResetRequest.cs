using System;

namespace Core.DomainModel
{
    public class PasswordResetRequest
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public DateTime Time { get; set; }
        public int User_Id { get; set; }

        public virtual User User { get; set; }
    }
}