using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Presentation.Web.Filters;

namespace Presentation.Web.Models
{
    public class HandoverDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(Rfc3339FullDateConverter))]
        public DateTime? MeetingDate { get; set; }
        public string Summary { get; set; }
        public virtual IEnumerable<UserDTO> Participants { get; set; }
    }
}
