using Presentation.Web.Infrastructure.Attributes;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class ArchiveLocationsController : BaseOptionController<ArchiveLocation, ItSystemUsage>
    {
        public ArchiveLocationsController(IGenericRepository<ArchiveLocation> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}