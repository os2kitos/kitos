using System;

namespace Presentation.Web.Models.API.V2.Request.Shared
{
    public class UpdateExternalReferenceDataWriteRequestDTO : ExternalReferenceDataWriteRequestDTO
    {
        //TODO: add summary
        public override Guid? Uuid { get; set; }
    }
}