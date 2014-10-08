using System;

namespace UI.MVC4.Models
{
    public class RightOutputDTO
    {
        // organization id
        public int ObjectId { get; set; }
        // organization name
        public string ObjectName { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
    }
}