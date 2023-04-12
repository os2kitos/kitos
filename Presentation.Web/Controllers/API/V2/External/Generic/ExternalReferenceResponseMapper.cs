using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class ExternalReferenceResponseMapper: IExternalReferenceResponseMapper
    {
        public IEnumerable<ExternalReferenceDataResponseDTO> MapExternalReferences(IEnumerable<ExternalReference> externalReferences)
        {
            return externalReferences.Select(MapExternalReference).ToList();
        }

        public ExternalReferenceDataResponseDTO MapExternalReference(ExternalReference externalReference)
        {
            return new ExternalReferenceDataResponseDTO
            {
                Uuid = externalReference.Uuid,
                DocumentId = externalReference.ExternalReferenceId,
                Title = externalReference.Title,
                Url = externalReference.URL,
                MasterReference = externalReference.IsMasterReference()
            };
        }
    }
}