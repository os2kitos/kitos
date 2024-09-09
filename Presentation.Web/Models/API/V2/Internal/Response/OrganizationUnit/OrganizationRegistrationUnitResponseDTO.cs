using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Internal.Common;

namespace Presentation.Web.Models.API.V2.Internal.Response.OrganizationUnit
{
    public class OrganizationRegistrationUnitResponseDTO
    {
        public IEnumerable<NamedEntityWithUserFullNameV2DTO> OrganizationUnitRights { get; set; } = new List<NamedEntityWithUserFullNameV2DTO>();
        public IEnumerable<NamedEntityV2DTO> ItContractRegistrations { get; set; } = new List<NamedEntityV2DTO>();
        public IEnumerable<PaymentRegistrationResponseDTO> Payments { get; set; } = new List<PaymentRegistrationResponseDTO>();
        public IEnumerable<NamedEntityWithEnabledStatusV2DTO> ResponsibleSystems { get; set; } = new List<NamedEntityWithEnabledStatusV2DTO>();
        public IEnumerable<NamedEntityWithEnabledStatusV2DTO> RelevantSystems { get; set; } = new List<NamedEntityWithEnabledStatusV2DTO>();
    }
}