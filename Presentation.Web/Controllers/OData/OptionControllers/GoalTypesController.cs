using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class GoalTypesController : BaseOptionController<GoalType, Goal>
    {
        public GoalTypesController(IGenericRepository<GoalType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}