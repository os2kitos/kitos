using Core.DomainModel.Archive;
using System.Linq;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Models.API.V2.Response.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public class ItSystemUsageArchiveResponseMapper : IItSystemUsageArchiveResponseMapper
    {
        public ItSystemUsageArchiveResponseDTO ToResponseDTO(ItSystemUsageArchive archive)
        {
            return new ItSystemUsageArchiveResponseDTO
            {
                Uuid = archive.Uuid,
                ArchivingDate = archive.ArchivingDate,
                ReferenceName = archive.ReferenceName,
                Note = archive.Note,
                ItSystemUuid = archive.Snapshot.ItSystemUuid,
                LegacyName = archive.Snapshot.LegacyName,
                TakenIntoUsageDate = archive.Snapshot.TakenIntoUsageDate,
                LocalName = archive.Snapshot.LocalName,
                LocalId = archive.Snapshot.LocalId,
                Organization = archive.Organization?.MapShallowOrganizationResponseDTO(),
                ArchivedByFullName = archive.ArchivedByUser.GetFullName(),
                ArchiveReferences = archive.ArchiveReferences?.Select(reference => new ArchiveReferenceResponseDTO
                {
                    Uuid = reference.Uuid,
                    Name = reference.Label,
                    Url = reference.Url
                }).ToList() ?? []
            };
        }
    }
}
