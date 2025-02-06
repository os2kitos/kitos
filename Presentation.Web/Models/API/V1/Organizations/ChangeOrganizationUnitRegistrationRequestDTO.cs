using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ChangeOrganizationUnitRegistrationRequestDTO
    {
        public IEnumerable<int> OrganizationUnitRights { get; set; } = new List<int>();
        public IEnumerable<int> ItContractRegistrations { get; set; } = new List<int>();
        public IEnumerable<ChangePaymentRegistrationRequestDTO> PaymentRegistrationDetails { get; set; } = new List<ChangePaymentRegistrationRequestDTO>();
        public IEnumerable<int> ResponsibleSystems { get; set; } = new List<int>();
        public IEnumerable<int> RelevantSystems { get; set; } = new List<int>();
    }
}