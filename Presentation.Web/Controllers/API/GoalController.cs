using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class GoalController : GenericApiController<Goal, GoalDTO>
    {
        public GoalController(IGenericRepository<Goal> repository) : base(repository)
        {
        }
    }
}
