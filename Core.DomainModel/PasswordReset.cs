using System;

namespace Core.DomainModel
{
    //TODO: perhaps this should be called something else, like reset request or something??
    public class PasswordReset
    {
        public int Id { get; set; }
        public string Hash { get; set; }
        public DateTime Time { get; set; }
        public User User { get; set; }
    }
}