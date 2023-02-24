using System.Collections.Generic;

namespace Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences
{
    public interface IHasExternalReferencesCreation
    {
        public IEnumerable<ExternalReferenceDataWriteRequestDTO> ExternalReferences { get; set; }
    }
}
