using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractSupplierDataResponseDTO
    {
        /// <summary>
        /// Optional reference to the supplier organization
        /// </summary>
        public ShallowOrganizationResponseDTO? Organization { get; set; }
        public IdentityNamePairResponseDTO? OrganizationUnit { get; set; }
        public bool? IsInternal { get; set; }
        /// <summary>
        /// Supplier type (Internal or External)
        /// </summary>
        public ItContractSupplierTypeChoice SupplierType { get; set; }
        /// <summary>
        /// Determines if the contract has been signed by the supplier
        /// </summary>
        [Required]
        public bool Signed { get; set; }
        /// <summary>
        /// Who, at the supplier, signed the contract
        /// </summary>
        public string? SignedBy { get; set; }
        /// <summary>
        /// Which date was the contract signed by the supplier
        /// </summary>
        public DateTime? SignedAt { get; set; }
        public string? ContactPerson { get; set; }
        public bool UseSignedByForContact { get; set; }
        public string? ContactPhoneNumber { get; set; }
        public string? ContactEmail { get; set; }
    }
}