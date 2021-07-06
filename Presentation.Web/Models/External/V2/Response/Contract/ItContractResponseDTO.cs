using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Presentation.Web.Models.External.V2.Response.Contract
{
    public class ItContractResponseDTO
    {
        /// <summary>
        /// UUID for IT-Contract
        /// </summary>
        public Guid Uuid { get; set; }

        /// <summary>
        /// Name of IT-Contract
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Contract type of IT-Contract
        /// </summary>
        public IdentityNamePairResponseDTO? ContractType { get; set; }

        /// <summary>
        /// Supplier of It-Contract
        /// </summary>
        public IdentityNamePairResponseDTO? Supplier { get; set; }

        /// <summary>
        /// Boolean determining if the IT-Contract is about system operation 
        /// </summary>
        public bool InOperation { get; set; }

        /// <summary>
        /// Date when IT-Contract is entered into agreement 
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Date when IT-Contract will expire
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Date when IT-Contract is terminated
        /// </summary>
        public DateTime? TerminatedAt { get; set; }
    }
}