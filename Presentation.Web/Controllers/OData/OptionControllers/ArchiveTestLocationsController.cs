using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    using Core.ApplicationServices;
    using Core.DomainModel.ItSystem;
    using Core.DomainModel.ItSystemUsage;
    using Core.DomainServices;

    [InternalApi]
    public class ArchiveTestLocationsController : BaseOptionController<ArchiveTestLocation, ItSystemUsage>
    {
        public ArchiveTestLocationsController(IGenericRepository<ArchiveTestLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}