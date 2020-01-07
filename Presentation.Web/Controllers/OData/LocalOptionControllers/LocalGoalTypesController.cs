using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class LocalGoalTypesController : LocalOptionBaseController<LocalGoalType, Goal, GoalType>
    {
        public LocalGoalTypesController(IGenericRepository<LocalGoalType> repository, IAuthenticationService authService, IGenericRepository<GoalType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
