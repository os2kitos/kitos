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
        public string SignedBy { get; set; }
        /// <summary>
        /// Which date was the contract signed by the supplier
        /// </summary>
        public DateTime? SignedAt { get; set; }
    }
}