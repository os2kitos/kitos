using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class GoalTypesController : BaseOptionController<GoalType, Goal>
    {
        public GoalTypesController(IGenericRepository<GoalType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}