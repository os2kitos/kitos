using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractPaymentsDataResponseDTO
    {
        /// <summary>
        /// External payments
        /// </summary>
        public IEnumerable<PaymentResponseDTO> External { get; set; }
        /// <summary>
        /// Internal payments
        /// </summary>
        public IEnumerable<PaymentResponseDTO> Internal { get; set; }
    }
}