using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Presentation.Web.Models.API.V2.Response.Shared;

namespace Presentation.Web.Controllers.API.V2.External.Generic
{
    public class ExternalReferenceResponseMapper: IExternalReferenceResponseMapper
    {
        public IEnumerable<ExternalReferenceDataResponseDTO> MapExternalReferenceDtoList(IEnumerable<ExternalReference> externalReferences,
            ExternalReference masterReference)
        {
            return externalReferences.Select(x => MapExternalReferenceDto(x, masterReference)).ToList();
        }

        private ExternalReferenceDataResponseDTO MapExternalReferenceDto(ExternalReference externalReference, ExternalReference masterReference)
        {
            return new ExternalReferenceDataResponseDTO
            {
                Uuid = externalReference.Uuid,
                DocumentId = externalReference.ExternalReferenceId,
                Title = externalReference.Title,
                Url = externalReference.URL,
                MasterReference = masterReference?.Id.Equals(externalReference.Id) == true
            };
        }
    }
}