using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class TerminationDeadlineTypesController : BaseOptionController<TerminationDeadlineType, ItContract>
    {
        public TerminationDeadlineTypesController(IGenericRepository<TerminationDeadlineType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}