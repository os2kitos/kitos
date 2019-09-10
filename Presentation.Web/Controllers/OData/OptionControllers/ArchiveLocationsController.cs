using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    [InternalApi]
    public class ArchiveLocationsController : BaseOptionController<ArchiveLocation, ItSystemUsage>
    {
        public ArchiveLocationsController(IGenericRepository<ArchiveLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}