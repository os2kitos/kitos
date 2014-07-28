using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectPhaseController : GenericApiController<ItProjectPhase, ItProjectPhaseDTO>
    {
        public ItProjectPhaseController(IGenericRepository<ItProjectPhase> repository) 
            : base(repository)
        {
        }
    }
}
