using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Presentation.Web.Models.API.V2.Response.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public interface IItSystemUsageResponseMapper
    {
        ItSystemUsageResponseDTO MapSystemUsageDTO(ItSystemUsage systemUsage);
        GeneralSystemRelationResponseDTO MapGeneralSystemRelationDTO(SystemRelation systemRelation);
        OutgoingSystemRelationResponseDTO MapOutgoingSystemRelationDTO(SystemRelation systemRelation);
        IncomingSystemRelationResponseDTO MapIncomingSystemRelationDTO(SystemRelation systemRelation);
        public JournalPeriodResponseDTO MapJournalPeriodResponseDto(ArchivePeriod period);
    }
}
