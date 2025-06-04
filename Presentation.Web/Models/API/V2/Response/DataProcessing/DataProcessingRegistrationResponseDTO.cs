using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator, IHasLastModified, IHasOrganizationContext
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid Uuid { get; set; }
        [Required]
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        [Required]
        public DateTime LastModified { get; set; }
        [Required]
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        [Required]
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
        [Required]
        public DataProcessingRegistrationGeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// Associated it-system-usage entities
        [Required]
        public IEnumerable<IdentityNamePairResponseDTO> SystemUsages { get; set; }
        [Required]
        public DataProcessingRegistrationOversightResponseDTO Oversight { get; set; }
        /// <summary>
        /// Data processing role assignments
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