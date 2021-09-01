using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Web.Models.API.V1
{
    /// <summary>
    /// DTO for listing contracts under IT systems module
    /// </summary>
    public class ItContractSystemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ItContractId { get; set; }

        public string ContractTypeName { get; set; }

        public string SupplierName { get; set; }
        
        /// <summary>
        /// When the contract began (indgået)
        /// </summary>
        public DateTime? Concluded { get; set; }
        /// <summary>
        /// When the contract expires (udløbet)
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// When the contract ends (opsagt)
        /// </summary>
        public DateTime? Terminated { get; set; }

        /// <summary>
        /// Whether the contract is active or not
        /// </summary>
        public bool IsActive { get; set; }

        public IEnumerable<OptionDTO> AgreementElements { get; set; }

        /// <summary>
        /// Whether the contract includes Operation agreement element ("drift")
        /// </summary>
        public bool HasOperationElement
        {
            get { return AgreementElements.Any(option => option.Name == "Drift"); }
        }
    }
}
