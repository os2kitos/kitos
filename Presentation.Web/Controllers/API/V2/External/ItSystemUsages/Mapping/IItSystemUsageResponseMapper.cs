using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Response.Shared;
using Presentation.Web.Models.API.V2.Response.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public interface IItSystemUsageResponseMapper
    {
        ItSystemUsageResponseDTO MapSystemUsageDTO(ItSystemUsage systemUsage);
        OutgoingSystemRelationResponseDTO MapOutgoingSystemRelationDTO(SystemRelation systemRelation);
        IncomingSystemRelationResponseDTO MapIncomingSystemRelationDTO(SystemRelation systemRelation);
        ExternalReferenceDataResponseDTO MapExternalReferenceDTO(ExternalReference externalReference);
    }
}
