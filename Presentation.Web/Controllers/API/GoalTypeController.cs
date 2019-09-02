using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class GoalTypeController : GenericOptionApiController<GoalType, Goal, OptionDTO>
    {
        public GoalTypeController(IGenericRepository<GoalType> repository) : base(repository)
        {
        }
    }
}
