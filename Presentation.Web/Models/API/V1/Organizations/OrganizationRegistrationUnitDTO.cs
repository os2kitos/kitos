using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class OrganizationRegistrationUnitDTO
    {
        public OrganizationRegistrationUnitDTO()
        {
            OrganizationUnitRights = new List<NamedEntityWithUserFullNameDTO>();
            ItContractRegistrations = new List<NamedEntityDTO>();
            Payments = new List<PaymentRegistrationDTO>();
            RelevantSystems = new List<NamedEntityWithEnabledStatusDTO>();
            ResponsibleSystems = new List<NamedEntityWithEnabledStatusDTO>();
        }

        public IEnumerable<NamedEntityWithUserFullNameDTO> OrganizationUnitRights { get; set; }
        public IEnumerable<NamedEntityDTO> ItContractRegistrations { get; set; }
        public IEnumerable<PaymentRegistrationDTO> Payments { get; set; }
        public IEnumerable<NamedEntityWithEnabledStatusDTO> ResponsibleSystems { get; set; }
        public IEnumerable<NamedEntityWithEnabledStatusDTO> RelevantSystems { get; set; }
    }
}