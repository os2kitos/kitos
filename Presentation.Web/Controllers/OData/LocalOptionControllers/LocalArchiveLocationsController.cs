using Presentation.Web.Infrastructure.Attributes;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class LocalArchiveLocationsController : LocalOptionBaseController<LocalArchiveLocation, ItSystemUsage, ArchiveLocation>
    {
        public LocalArchiveLocationsController(IGenericRepository<LocalArchiveLocation> repository, IGenericRepository<ArchiveLocation> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}