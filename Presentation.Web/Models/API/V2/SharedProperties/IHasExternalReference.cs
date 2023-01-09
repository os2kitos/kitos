using System.Collections.Generic;
using Presentation.Web.Models.API.V2.Types.Shared;

namespace Presentation.Web.Models.API.V2.SharedProperties
{
    public interface IHasExternalReference<T> where T : BaseExternalReferenceDTO
    {
        IEnumerable<T> ExternalReferences{ get; set; }
    }
}