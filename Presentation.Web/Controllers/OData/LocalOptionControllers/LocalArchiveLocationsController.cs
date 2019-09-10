using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainModel.LocalOptions;
    using Core.DomainServices;

    [InternalApi]
    public class LocalArchiveLocationsController : LocalOptionBaseController<LocalArchiveLocation, ItSystemUsage, ArchiveLocation>
    {
        public LocalArchiveLocationsController(IGenericRepository<LocalArchiveLocation> repository, IAuthenticationService authService, IGenericRepository<ArchiveLocation> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}