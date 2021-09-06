using System;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractTerminationDataResponseDTO
    {
        /// <summary>
        /// The date at which the contract was terminated
        /// </summary>
        public DateTime? TerminatedAt { get; set; }
        /// <summary>
        /// Contract termination terms
        /// </summary>
        public ContractTerminationTermsResponseDTO Terms { get; set; }
    }
}