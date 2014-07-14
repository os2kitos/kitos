using System;
using Newtonsoft.Json;
using UI.MVC4.Filters;

namespace UI.MVC4.Models
{
    public class RightOutputDTO
    {
        public int ObjectId { get; set; }

        public int RoleId { get; set; }

        public int UserId { get; set; }
        public UserDTO User { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime LastChanged { get; set; }
        public int LastChangedByUserId { get; set; }
    }
}