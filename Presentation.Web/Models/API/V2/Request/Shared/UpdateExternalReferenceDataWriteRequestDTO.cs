using System;

namespace Presentation.Web.Models.API.V2.Request.Shared
{
    public class UpdateExternalReferenceDataWriteRequestDTO : ExternalReferenceDataWriteRequestDTO
    {
        /// <summary>
        /// The UUID of the External Reference
        /// Constrains:
        ///     - If the reference has a uuid it the update points to an existing reference (with the same uuid).
        ///     - If the reference has no uuid, it will be considered anonymous and be added as such (and KITOS will assign a uuid to it automatically)
        /// </summary>
        public Guid? Uuid { get; set; }
    }
}