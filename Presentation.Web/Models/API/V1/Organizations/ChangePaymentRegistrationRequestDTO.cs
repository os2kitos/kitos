using System.Collections.Generic;

namespace Presentation.Web.Models.API.V1.Organizations
{
    public class ChangePaymentRegistrationRequestDTO
    {
        public ChangePaymentRegistrationRequestDTO()
        {
            InternalPayments = new List<int>();
            ExternalPayments = new List<int>();
        }

        public int ItContractId { get; set; }
        public IEnumerable<int> InternalPayments { get; set; }
        public IEnumerable<int> ExternalPayments { get; set; }
    }
}