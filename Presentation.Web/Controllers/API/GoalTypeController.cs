using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class GoalTypeController : GenericOptionApiController<GoalType, Goal, OptionDTO>
    {
        public GoalTypeController(IGenericRepository<GoalType> repository) : base(repository)
        {
        }
    }
}
