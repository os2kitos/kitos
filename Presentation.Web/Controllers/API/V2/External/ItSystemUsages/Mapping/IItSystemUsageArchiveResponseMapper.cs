using Core.DomainModel.Archive;
using Presentation.Web.Models.API.V2.Response.SystemUsage;

namespace Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping
{
    public interface IItSystemUsageArchiveResponseMapper
    {
        ItSystemUsageArchiveResponseDTO ToResponseDTO(ItSystemUsageArchive archive);
    }
}
