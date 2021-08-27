using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class DataProcessingRegistrationWriteRequestDTO
    {
        public GeneralDataWriteRequestDTO General { get; set; }
        /// <summary>
        /// UUIDs of associated it-system-usage entities
        /// Constraints:
        ///     - No duplicates
        ///     - System usages must be belong to the same organization as this data processing registration
        /// </summary>
        public IEnumerable<Guid> SystemUuids { get; set; }
        public OversightWriteRequestDTO Oversight { get; set; }
        /// <summary>
        /// Data processing role assignments
        /// Constraints:
        ///     - Users must be members of the same organization as this data processing registration
        ///     - Role options must be available in the organization of the data processing registration
        /// </summary>
        public IEnumerable<RoleAssignmentRequestDTO> Roles { get; set; }
        /// <summary>
        /// External reference definitions
        /// </summary>
        public IEnumerable<ExternalReferenceDataDTO> ExternalReferences { get; set; }
    }
}