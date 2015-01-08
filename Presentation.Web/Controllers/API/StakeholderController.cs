using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class StakeholderController : GenericApiController<Stakeholder, StakeholderDTO>
    {
        public StakeholderController(IGenericRepository<Stakeholder> repository) : base(repository)
        {
        }
    }
}
