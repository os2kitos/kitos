using System;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ContractResponsibleDataResponseDTO
    {
        /// <summary>
        /// Optional reference responsible organization unit
        /// </summary>
        public IdentityNamePairResponseDTO OrganizationUnit { get; set; }
        /// <summary>
        /// Determines if the contract has been signed
        /// </summary>
        [Required]
        public bool Signed { get; set; }
        /// <summary>
        /// Who, in the organization, signed the contract
        /// </summary>
        public string SignedBy { get; set; }
        /// <summary>
        /// Which date was the contract signed
        /// </summary>
        public DateTime? SignedAt { get; set; }
    }
}