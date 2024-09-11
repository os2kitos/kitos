using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Internal.Request.OrganizationUnit
{
    public class ChangePaymentRegistrationV2RequestDTO
    {
        public int ItContractId { get; set; }
        public IEnumerable<int> InternalPayments { get; set; } = new List<int>();
        public IEnumerable<int> ExternalPayments { get; set; } = new List<int>();
    }
}