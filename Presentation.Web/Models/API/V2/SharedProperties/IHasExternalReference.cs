using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Request.Generic.ExternalReferences;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasExternalReference<T> where T : ExternalReferenceDataWriteRequestDTO
    {
        IEnumerable<T> ExternalReferences{ get; set; }
    }
}