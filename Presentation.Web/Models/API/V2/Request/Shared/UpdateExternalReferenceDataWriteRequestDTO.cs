using System;

namespace Presentation.Web.Models.API.V2.Request.Shared
{
    public class UpdateExternalReferenceDataWriteRequestDTO : ExternalReferenceDataWriteRequestDTO
    {
        /// <summary>
        /// The UUID of the External Reference
        /// Constrains:
        ///     - If the reference has a uuid it will update an existing reference (with the same uuid), uuid must exist
        ///     - If the reference has no uuid, a new External Reference will be created
        /// </summary>
        public Guid? Uuid { get; set; }
    }
}