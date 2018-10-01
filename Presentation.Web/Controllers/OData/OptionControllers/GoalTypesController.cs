using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GoalTypesController : BaseOptionController<GoalType, Goal>
    {
        public GoalTypesController(IGenericRepository<GoalType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}