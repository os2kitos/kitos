using System;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractSupplierDataWriteRequestDTO
    {
        /// <summary>
        /// Optional reference to the supplier organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? OrganizationUuid { get; set; }
        /// <summary>
        /// Determines if the contract has been signed by the supplier
        /// </summary>
        public bool Signed { get; set; }
        /// <summary>
        /// Who, at the supplier, signed the contract
        /// </summary>
        public string? SignedBy { get; set; }
        /// <summary>
        /// Which date was the contract signed by the supplier
        /// </summary>
        public DateTime? SignedAt { get; set; }

        /// <summary>
        /// Optional reference to the supplier organization unit
        /// requires IsInternal to be true
        /// </summary>
        public Guid? OrganizationUnitUuid { get; set; }

        /// <summary>
        /// Determines if the contract is internal (supplier is part of the same organization)
        /// </summary>
        public bool IsInternal { get; set; }
        
        /// <summary>
        /// Contact person at the supplier organization
        /// </summary>
        public string? ContactPerson { get; set; }
        /// <summary>
        /// Determines if the contact person is the same as the person who signed the contract
        /// </summary>
        public bool UseSignedByForContact { get; set; }

        /// <summary>
        /// Contact phone number at the supplier organization
        /// </summary>
        public string? ContactPhoneNumber { get; set; }
        /// <summary>
        /// Contact email at the supplier organization
        /// </summary>
        public string? ContactEmail { get; set; }
    }
}