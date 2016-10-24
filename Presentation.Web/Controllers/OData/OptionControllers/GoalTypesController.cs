using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class GoalTypesController : BaseEntityController<GoalType>
    {
        public GoalTypesController(IGenericRepository<GoalType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}