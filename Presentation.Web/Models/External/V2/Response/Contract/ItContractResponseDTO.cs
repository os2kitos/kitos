using Presentation.Web.Models.External.V2.SharedProperties;
using System;
using System.Collections.Generic;

namespace Presentation.Web.Models.External.V2.Response.Contract
{
    public class ItContractResponseDTO : IHasNameExternal, IHasUuidExternal, IHasValidationExternal
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
        /// Contract type of IT-Contract
        /// </summary>
        public IdentityNamePairResponseDTO ContractType { get; set; }

        /// <summary>
        /// Supplier of It-Contract
        /// </summary>
        public ShallowOrganizationDTO Supplier { get; set; }

        /// <summary>
        /// Agreement element option types set on the IT-Contract
        /// </summary>
        public IEnumerable<IdentityNamePairResponseDTO> AgreementElements { get; set; }

        /// <summary>
        /// Date when IT-Contract is entered into agreement 
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Date when IT-Contract will expire
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Date when IT-Contract is terminated
        /// </summary>
        public DateTime? TerminatedAt { get; set; }

        /// <summary>
        /// Whether the IT-Contract is active or not
        /// </summary>
        public bool IsValid { get; set; }
    }
}