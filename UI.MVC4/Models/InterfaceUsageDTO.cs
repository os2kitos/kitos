using System.Collections.Generic;

namespace UI.MVC4.Models
{
    public class InterfaceUsageDTO
    {
        public int Id { get; set; }
        public int ItSystemUsageId { get; set; }
        public ItSystemUsageSimpleDTO ItSystemUsage { get; set; }

        public ItSystemDTO Interface { get; set; }

        public IEnumerable<DataRowUsageDTO> DataRowUsages { get; set; }

        public int? ItContractId { get; set; }
        public ItContractSystemDTO ItContract { get; set; }

        public int? InfrastructureId { get; set; }
        public string InfrastructureItSystemName { get; set; }

        public bool IsWishedFor { get; set; }
        
        public int ItInterfaceId { get; set; }
        public int ItSystemId { get; set; }

        public string ItInterfaceUseItSystemName { get; set; }
        public string ItInterfaceUseItInterfaceName { get; set; }
    }
}