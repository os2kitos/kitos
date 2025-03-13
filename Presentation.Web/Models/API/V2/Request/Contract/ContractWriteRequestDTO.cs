using System;
using System.Collections.Generic;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;

namespace Presentation.Web.Models.API.V2.Request.Contract
{
    public class ContractWriteRequestDTO
    {
        /// <summary>
        /// UUID of the optional parent contract
        /// Constraints:
        ///     - Parent and child contract must belong to the same organization
        /// </summary>
        [NonEmptyGuid]
        public Guid? ParentContractUuid { get; set; }
        public ContractGeneralDataWriteRequestDTO General { get; set; }
        public ContractProcurementDataWriteRequestDTO Procurement { get; set; }
        public ContractSupplierDataWriteRequestDTO Supplier { get; set; }
        public ContractResponsibleDataWriteRequestDTO Responsible { get; set; }
        /// <summary>
        /// IT-System usages covered by this it-contract
        /// Constraints:
        ///     - System usages must belong to the same organization as the it-contract
        ///     - No duplicates
        /// </summary>
        public IEnumerable<Guid> SystemUsageUuids { get; set; }
        /// <summary>
        /// Data processing registrations associated with this it-contract
        /// Constraints:
        ///     - Must belong to the same organization as the it-contract
        ///     - No duplicates
        /// </summary>
        public IEnumerable<Guid> DataProcessingRegistrationUuids { get; set; }

        public ContractPaymentModelDataWriteRequestDTO PaymentModel { get; set; }
        public ContractAgreementPeriodDataWriteRequestDTO AgreementPeriod { get; set; }
        public ContractTerminationDataWriteRequestDTO Termination { get; set; }
        public ContractPaymentsDataWriteRequestDTO Payments { get; set; }

        /// <summary>
        /// Role assignments
        /// Constraints:
        ///     - No duplicates
        /// </summary>
        public IEnumerable<RoleAssignmentRequestDTO> Roles { get; set; }

    }
}