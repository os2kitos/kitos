using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectStatusController : GenericApiController<ItProjectStatus, ItProjectStatusDTO>
    {
        public ItProjectStatusController(IGenericRepository<ItProjectStatus> repository) 
            : base(repository)
        {
        }
    }
}
