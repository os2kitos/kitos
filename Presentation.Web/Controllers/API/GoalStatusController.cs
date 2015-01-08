using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class GoalStatusController : GenericApiController<GoalStatus, GoalStatusDTO>
    {
        public GoalStatusController(IGenericRepository<GoalStatus> repository) : base(repository)
        {
        }
    }
}
