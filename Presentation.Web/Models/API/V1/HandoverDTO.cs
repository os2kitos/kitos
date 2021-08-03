using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1
{
    public class HandoverDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string Summary { get; set; }
        public virtual IEnumerable<UserDTO> Participants { get; set; }
    }
}
