using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class ArchiveLocationsController : BaseOptionController<ArchiveLocation, ItSystemUsage>
    {
        public ArchiveLocationsController(IGenericRepository<ArchiveLocation> repository)
            : base(repository)
        {
        }
    }
}