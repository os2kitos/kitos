using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ArchiveTestLocationsController : BaseOptionController<ArchiveTestLocation, ItSystemUsage>
    {
        public ArchiveTestLocationsController(IGenericRepository<ArchiveTestLocation> repository)
            : base(repository)
        {
        }
    }
}