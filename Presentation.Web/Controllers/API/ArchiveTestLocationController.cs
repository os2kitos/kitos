using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API
{
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    using Presentation.Web.Models;

    [PublicApi]
    [DeprecatedApi]
    public class ArchiveTestLocationController : GenericOptionApiController<ArchiveTestLocation, ItSystemUsage, OptionDTO>
    {
        /// <summary>
        /// Arkiveringsteststed fra it system anvendelsen, arkiverings fanen
        /// </summary>
        /// <param name="repository"></param>
        public ArchiveTestLocationController(IGenericRepository<ArchiveTestLocation> repository)
            : base(repository)
        {
        }
    }
}