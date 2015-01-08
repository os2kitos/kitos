using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItProjectPhaseController : GenericApiController<ItProjectPhase, ItProjectPhaseDTO>
    {
        public ItProjectPhaseController(IGenericRepository<ItProjectPhase> repository) 
            : base(repository)
        {
        }
    }
}
