using Presentation.Web.Models.API.V2.SharedProperties;
using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ItContractResponseDTO : IHasNameExternal, IHasUuidExternal, IHasLastModified, IHasEntityCreator, IHasOrganizationContext
    {
        /// <summary>
        /// UUID for IT-Contract
        /// </summary>
        public required Guid Uuid { get; set; }
        /// <summary>
        /// Name of IT-Contract
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Organization in which the contract was created
        /// </summary>
        public ShallowOrganizationResponseDTO? OrganizationContext { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        public required DateTime LastModified { get; set; }
        /// <summary>
        /// Reference to the user who last modified the contract
        /// </summary>
        public IdentityNamePairResponseDTO? LastModifiedBy { get; set; }
        /// <summary>
        /// Reference to the user who created the contract
        /// </summary>
        public IdentityNamePairResponseDTO? CreatedBy { get; set; }
        /// <summary>
        /// Optional parent contract
        /// </summary>
        public IdentityNamePairResponseDTO? ParentContract { get; set; }
        public required ContractGeneralDataResponseDTO General { get; set; }
        public required ContractProcurementDataResponseDTO Procurement { get; set; }
        public required ContractSupplierDataResponseDTO Supplier { get; set; }
        public required ContractResponsibleDataResponseDTO Responsible { get; set; }
        /// <summary>
        /// Associated IT-System usages
        /// </summary>
        public required List<IdentityNamePairResponseDTO?> SystemUsages { get; set; }
        /// <summary>
        /// Data processing registrations associated with this it-contract
        /// </summary>
        public required IEnumerable<IdentityNamePairResponseDTO> DataProcessingRegistrations { get; set; }
        public required ContractPaymentModelDataResponseDTO PaymentModel { get; set; }
        public required ContractAgreementPeriodDataResponseDTO AgreementPeriod { get; set; }
        public required ContractTerminationDataResponseDTO Termination { get; set; }
        public required ContractPaymentsDataResponseDTO Payments { get; set; }
        /// <summary>
        /// Role assignments
        /// </summary>
        public required IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public required IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
    }
}