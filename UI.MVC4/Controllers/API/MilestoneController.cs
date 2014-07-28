using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class MilestoneController : GenericApiController<Milestone, MilestoneDTO>
    {
        public MilestoneController(IGenericRepository<Milestone> repository) 
            : base(repository)
        {
        }
    }
}
