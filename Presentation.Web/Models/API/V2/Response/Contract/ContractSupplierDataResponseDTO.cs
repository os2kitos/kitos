using System;
using Presentation.Web.Models.API.V2.Response.Organization;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractSupplierDataResponseDTO
    {
        /// <summary>
        /// Optional reference to the supplier
        /// </summary>
        public ShallowOrganizationResponseDTO SupplierOrganization { get; set; }
        /// <summary>
        /// Determines if the contract has been signed by the supplier
        /// </summary>
        public bool Signed { get; set; }
        /// <summary>
        /// Who, at the supplier, signed the contract
        /// </summary>
        public string SignedBy { get; set; }
        /// <summary>
        /// Which date was the contract signed by the supplier
        /// </summary>
        public DateTime? SignedAt { get; set; }
    }
}