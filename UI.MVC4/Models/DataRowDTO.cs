using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class DataRowDTO
    {
        public int Id { get; set; }
        public int ItSystemId { get; set; }
        public string Data { get; set; }
        public int? DataTypeId { get; set; }
        public int ItInterfaceId { get; set; }
        public IEnumerable<DataRowUsageDTO> Usages { get; set; }
    }
}