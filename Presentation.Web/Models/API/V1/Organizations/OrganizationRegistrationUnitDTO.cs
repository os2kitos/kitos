using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class OrganizationRegistrationUnitDTO
    {
        public IEnumerable<NamedEntityWithUserFullNameDTO> OrganizationUnitRights { get; set; } = new List<NamedEntityWithUserFullNameDTO>();
        public IEnumerable<NamedEntityDTO> ItContractRegistrations { get; set; } = new List<NamedEntityDTO>();
        public IEnumerable<PaymentRegistrationDTO> Payments { get; set; } = new List<PaymentRegistrationDTO>();
        public IEnumerable<NamedEntityWithEnabledStatusDTO> ResponsibleSystems { get; set; } = new List<NamedEntityWithEnabledStatusDTO>();
        public IEnumerable<NamedEntityWithEnabledStatusDTO> RelevantSystems { get; set; } = new List<NamedEntityWithEnabledStatusDTO>();
    }
}