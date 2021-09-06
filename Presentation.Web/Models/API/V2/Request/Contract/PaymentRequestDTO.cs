using System;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Types.Contract;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class PaymentRequestDTO
    {
        /// <summary>
        /// Optionally defined the organization unit responsible for the payment
        /// Constraints:
        ///     - Organization unit must belong to the same organization as the contract
        /// </summary>
        [NonEmptyGuid]
        public Guid? OrganizationUnitUuid { get; set; }
        /// <summary>
        /// Part of payment which covers acquisition
        /// </summary>
        public int Acquisition { get; set; }
        /// <summary>
        /// Part of payment which covers operations
        /// </summary>
        public int Operation { get; set; }
        /// <summary>
        /// Part of payment which is not classified as either operations or acquisition
        /// </summary>
        public int Other { get; set; }
        public string AccountingEntry { get; set; }
        /// <summary>
        /// The result of the specific payment audit
        /// </summary>
        public PaymentAuditStatus AuditStatus { get; set; }
        /// <summary>
        /// Defines the date at which the payment was audited
        /// </summary>
        public DateTime? AuditDate { get; set; }
        public string Note { get; set; }
    }
}