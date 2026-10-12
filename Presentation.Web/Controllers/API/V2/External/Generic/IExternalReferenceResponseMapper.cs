using System.Collections.Generic;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public interface IExternalReferenceResponseMapper
    {
        ExternalReferenceDataResponseDTO MapExternalReference(ExternalReference externalReference);
        IEnumerable<ExternalReferenceDataResponseDTO> MapExternalReferences(IEnumerable<ExternalReference> externalReferences);
    }
}
