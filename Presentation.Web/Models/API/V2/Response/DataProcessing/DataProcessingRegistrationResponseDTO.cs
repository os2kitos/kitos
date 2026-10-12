using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Response.Generic.Identity;
using Presentation.Web.Models.API.V2.Response.Generic.Roles;
using Presentation.Web.Models.API.V2.Response.Organization;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.SharedProperties;
using Presentation.Web.Models.API.V2.Types.DataProcessing;

namespace Presentation.Web.Models.API.V2.Response.DataProcessing
{
    public class DataProcessingRegistrationResponseDTO : IHasNameExternal, IHasUuidExternal, IHasEntityCreator, IHasLastModified, IHasOrganizationContext
    {
        public required string Name { get; set; }
        public required Guid Uuid { get; set; }
        public IdentityNamePairResponseDTO? CreatedBy { get; set; }
        /// <summary>
        /// UTC timestamp of latest modification
        /// </summary>
        public required DateTime LastModified { get; set; }
        public IdentityNamePairResponseDTO? LastModifiedBy { get; set; }
        public ShallowOrganizationResponseDTO? OrganizationContext { get; set; }
        public required DataProcessingRegistrationGeneralDataResponseDTO General { get; set; }
        /// <summary>
        /// Associated it-system-usage entities
        /// </summary>
        public required IEnumerable<ShallowItSystemUsageResponseDTO> SystemUsages { get; set; }
        public required DataProcessingRegistrationOversightResponseDTO Oversight { get; set; }
        /// <summary>
        /// Data processing role assignments
        /// </summary>
        public required IEnumerable<RoleAssignmentResponseDTO> Roles { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public required IEnumerable<ExternalReferenceDataResponseDTO> ExternalReferences { get; set; }
    }
}