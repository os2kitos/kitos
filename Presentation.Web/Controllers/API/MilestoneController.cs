using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class MilestoneController : GenericContextAwareApiController<Milestone, MilestoneDTO>
    {
        public MilestoneController(IGenericRepository<Milestone> repository)
            : base(repository)
        {
        }
    }
}
