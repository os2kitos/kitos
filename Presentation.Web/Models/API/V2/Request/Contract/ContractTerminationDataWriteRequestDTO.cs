using System;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractTerminationDataWriteRequestDTO
    {
        /// <summary>
        /// The date at which the contract was terminated
        /// </summary>
        public DateTime? TerminatedAt { get; set; }
        /// <summary>
        /// Contract termination terms
        /// </summary>
        public ContractTerminationTermsRequestDTO Terms { get; set; }
    }
}