using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class InterfaceUsageDTO
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public int InterfaceId { get; set; }
        public IEnumerable<DataRowUsageDTO> DataRowUsages { get; set; }

        public int? InfrastructureId { get; set; }
        public string InfrastructureName { get; set; }

        public int? InterfaceCategoryId { get; set; }
    }
}