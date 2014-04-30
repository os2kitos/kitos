using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class InterfaceUsageDTO
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public int InterfaceId { get; set; }
        public virtual ICollection<DataRowUsageDTO> DataRowUsages { get; set; } 
    }
}