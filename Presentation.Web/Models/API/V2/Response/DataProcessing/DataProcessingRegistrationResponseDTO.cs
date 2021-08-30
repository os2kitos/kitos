using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator, IHasLastModified
    {
        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public IdentityNamePairResponseDTO CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public IdentityNamePairResponseDTO LastModifiedBy { get; set; }
        public ShallowOrganizationResponseDTO OrganizationContext { get; set; }
        public DataProcessingRegistrationGeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// Associated it-system-usage entities
        public IEnumerable<IdentityNamePairResponseDTO> SystemUsageUuids { get; set; }
        public DataProcessingRegistrationOversightResponseDTO Oversight { get; set; }
        /// <summary>
        /// Data processing role assignments
        /// </summary>
        public IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public IEnumerable<ExternalReferenceDataDTO> ExternalReferences { get; set; }
    }
}