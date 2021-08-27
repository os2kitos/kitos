using System;
using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.Roles;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Request.DataProcessing
{
    public class DataProcessingRegistrationWriteRequestDTO
    {
        public GeneralDataWriteRequestDTO General { get; set; }
        public IEnumerable<Guid> SystemUuids { get; set; }
        public OversightWriteRequestDTO Oversight { get; set; }
        public IEnumerable<RoleAssignmentRequestDTO> Roles { get; set; }
        public IEnumerable<ExternalReferenceDataDTO> ExternalReferences { get; set; }
    }
}