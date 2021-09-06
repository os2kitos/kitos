using System;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractResponsibleDataWriteRequestDTO
    {
        /// <summary>
        /// Optional reference responsible organization unit
        /// Constraints:
        ///     - Must be a organization unit of the Organization to which the contract belongs
        /// </summary>
        [NonEmptyGuid]
        public Guid? OrganizationUnitUuid { get; set; }
        /// <summary>
        /// Determines if the contract has been signed
        /// </summary>
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