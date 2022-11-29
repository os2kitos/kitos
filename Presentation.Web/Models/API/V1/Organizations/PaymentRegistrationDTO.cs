using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class PaymentRegistrationDTO
    {
        public PaymentRegistrationDTO()
        {
            ItContract = new NamedEntityDTO();
            InternalPayments = new List<NamedEntityDTO>();
            ExternalPayments = new List<NamedEntityDTO>();
        }
        public NamedEntityDTO ItContract { get; set; }
        public IEnumerable<NamedEntityDTO> InternalPayments { get; set; }
        public IEnumerable<NamedEntityDTO> ExternalPayments { get; set; }
    }
}