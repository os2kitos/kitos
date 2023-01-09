using Presentation.Web.Models.API.V2.SharedProperties;
using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Models.API.V2.Response.Contract
{
    public class ItContractResponseDTO : IHasNameExternal, IHasUuidExternal, IHasLastModified, IHasEntityCreator
    {
        /// <summary>
        /// UUID for IT-Contract
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Name of IT-Contract
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Organization in which the contract was created
        /// </summary>
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        public DateTime LastModified { get; set; }
        /// <summary>
        /// Reference to the user who last modified the contract
        /// </summary>
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        /// <summary>
        /// Reference to the user who created the contract
        /// </summary>
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        /// <summary>
        /// Optional parent contract
        /// </summary>
        public IdentityNamePairResponseDTO ParentContract { get; set; }
        public ContractGeneralDataResponseDTO General { get; set; }
        public ContractProcurementDataResponseDTO Procurement { get; set; }
        public ContractSupplierDataResponseDTO Supplier { get; set; }
        public ContractResponsibleDataResponseDTO Responsible { get; set; }
        /// <summary>
        /// Associated IT-System usages
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> SystemUsages { get; set; }
        /// <summary>
        /// Data processing registrations associated with this it-contract
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> DataProcessingRegistrations { get; set; }
        public ContractPaymentModelDataResponseDTO PaymentModel { get; set; }
        public ContractAgreementPeriodDataResponseDTO AgreementPeriod { get; set; }
        public ContractTerminationDataResponseDTO Termination { get; set; }
        public ContractPaymentsDataResponseDTO Payments { get; set; }
        /// <summary>
        /// Role assignments
        /// </summary>
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        public IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
    }
}