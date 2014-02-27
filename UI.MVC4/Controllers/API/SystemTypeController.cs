using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class SystemTypeController : GenericApiController<SystemType, int>
    {
        public SystemTypeController(IGenericRepository<SystemType> repository) 
            : base(repository)
        {
        }
    }
}