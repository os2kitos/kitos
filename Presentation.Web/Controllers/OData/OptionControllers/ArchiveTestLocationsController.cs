using Presentation.Web.Infrastructure.Attributes;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class ArchiveTestLocationsController : BaseOptionController<ArchiveTestLocation, ItSystemUsage>
    {
        public ArchiveTestLocationsController(IGenericRepository<ArchiveTestLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}