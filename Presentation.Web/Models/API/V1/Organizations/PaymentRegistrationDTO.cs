using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class PaymentRegistrationDTO
    {
        public NamedEntityDTO ItContract { get; set; } = new();
        public IEnumerable<NamedEntityDTO> InternalPayments { get; set; } = new List<NamedEntityDTO>();
        public IEnumerable<NamedEntityDTO> ExternalPayments { get; set; } = new List<NamedEntityDTO>();
    }
}