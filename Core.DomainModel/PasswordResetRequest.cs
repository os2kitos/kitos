using System;

namespace Core.DomainModel
{
    /// <summary>
    /// Entity for resetting the password of a user. 
    /// The request is issued by <see><cref>IUserService</cref></see>.
    /// </summary>
    public class PasswordResetRequest : Entity
    {
        //Hashed identifier used to get the request
        public string Hash { get; set; }

        /// <summary>
        /// Time of the issue
        /// </summary>
        public DateTime Time { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}