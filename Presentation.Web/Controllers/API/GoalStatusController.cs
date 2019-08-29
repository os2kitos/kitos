using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class GoalStatusController : GenericContextAwareApiController<GoalStatus, GoalStatusDTO>
    {
        public GoalStatusController(IGenericRepository<GoalStatus> repository) : base(repository)
        {
        }
    }
}
