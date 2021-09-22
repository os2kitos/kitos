using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractPaymentsDataWriteRequestDTO
    {
        /// <summary>
        /// External payments
        /// </summary>
        public IEnumerable<PaymentRequestDTO> External { get; set; }
        /// <summary>
        /// Internal payments
        /// </summary>
        public IEnumerable<PaymentRequestDTO> Internal { get; set; }
    }
}