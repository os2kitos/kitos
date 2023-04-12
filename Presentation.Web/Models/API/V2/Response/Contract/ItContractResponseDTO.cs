using Presentation.Web.Models.API.V2.SharedProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required]
        public Guid Uuid { get; set; }
        /// <summary>
        /// Name of IT-Contract
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Organization in which the contract was created
        /// </summary>
        [Required]
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Reference to the user who last modified the contract
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        /// <summary>
        /// Reference to the user who created the contract
        /// </summary>
        [Required]
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        /// <summary>
        /// Optional parent contract
        /// </summary>
        public IdentityNamePairResponseDTO ParentContract { get; set; }
        [Required]
        public ContractGeneralDataResponseDTO General { get; set; }
        [Required]
        public ContractProcurementDataResponseDTO Procurement { get; set; }
        [Required]
        public ContractSupplierDataResponseDTO Supplier { get; set; }
        [Required]
        public ContractResponsibleDataResponseDTO Responsible { get; set; }
        /// <summary>
        /// Associated IT-System usages
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> SystemUsages { get; set; }
        /// <summary>
        /// Data processing registrations associated with this it-contract
        /// </summary>
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> DataProcessingRegistrations { get; set; }
        [Required]
        public ContractPaymentModelDataResponseDTO PaymentModel { get; set; }
        [Required]
        public ContractAgreementPeriodDataResponseDTO AgreementPeriod { get; set; }
        [Required]
        public ContractTerminationDataResponseDTO Termination { get; set; }
        [Required]
        public ContractPaymentsDataResponseDTO Payments { get; set; }
        /// <summary>
        /// Role assignments
        /// </summary>
        [Required]
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        [Required]
        public IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
    }
}