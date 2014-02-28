using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers
{
    public class SystemTypeController : GenericApiController<SystemType, int, SystemTypeDTO>
    {
        public SystemTypeController(IGenericRepository<SystemType> repository) 
            : base(repository)
        {
        }
    }
}