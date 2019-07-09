using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalGoalTypesController : LocalOptionBaseController<LocalGoalType, Goal, GoalType>
    {
        public LocalGoalTypesController(IGenericRepository<LocalGoalType> repository, IAuthenticationService authService, IGenericRepository<GoalType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
