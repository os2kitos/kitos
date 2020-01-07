using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class ItProjectTypesController : BaseOptionController<ItProjectType, ItProject>
    {
        public ItProjectTypesController(IGenericRepository<ItProjectType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}