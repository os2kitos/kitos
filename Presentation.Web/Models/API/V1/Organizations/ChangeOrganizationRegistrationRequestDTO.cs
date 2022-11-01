using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ChangeOrganizationRegistrationRequestDTO
    {
        public ChangeOrganizationRegistrationRequestDTO()
        {
            OrganizationUnitRights = new List<int>();
            ItContractRegistrations = new List<int>();
            PaymentRegistrationDetails = new List<ChangePaymentRegistraitonRequestDTO>();
            ResponsibleSystems = new List<int>();
            RelevantSystems = new List<int>();
        }

        public IEnumerable<int> OrganizationUnitRights { get; set; }
        public IEnumerable<int> ItContractRegistrations { get; set; }
        public IEnumerable<ChangePaymentRegistraitonRequestDTO> PaymentRegistrationDetails { get; set; }
        public IEnumerable<int> ResponsibleSystems { get; set; }
        public IEnumerable<int> RelevantSystems { get; set; }
    }
}