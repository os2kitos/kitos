using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractPaymentsDataResponseDTO
    {
        /// <summary>
        /// External payments
        /// </summary>
        [Required]
        public IEnumerable<PaymentResponseDTO> External { get; set; }
        /// <summary>
        /// Internal payments
        /// </summary>
        [Required]
        public IEnumerable<PaymentResponseDTO> Internal { get; set; }
    }
}