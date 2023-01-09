using System;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.Response.Shared
{
    public class ExternalReferenceDataResponseDTO : BaseExternalReferenceDTO
    {
        //TODO: add summary
        public Guid Uuid { get; set; }
    }
}