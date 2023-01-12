using System;

namespace Presentation.Web.Models.API.V2.Request.Shared
{
    public class UpdateExternalReferenceDataWriteRequestDTO : ExternalReferenceDataWriteRequestDTO
    {
        /// <summary>
        /// The UUID of the External Reference
        /// </summary>
        public Guid? Uuid { get; set; }
    }
}