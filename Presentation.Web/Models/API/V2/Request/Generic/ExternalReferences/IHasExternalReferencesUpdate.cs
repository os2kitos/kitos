using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences
{
    public interface IHasExternalReferencesUpdate
    {
        public IEnumerable<UpdateExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }

    }
}
