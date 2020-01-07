using Presentation.Web.Infrastructure.Attributes;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class LocalArchiveTestLocationsController : LocalOptionBaseController<LocalArchiveTestLocation, ItSystemUsage, ArchiveTestLocation>
    {
        public LocalArchiveTestLocationsController(IGenericRepository<LocalArchiveTestLocation> repository, IAuthenticationService authService, IGenericRepository<ArchiveTestLocation> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}