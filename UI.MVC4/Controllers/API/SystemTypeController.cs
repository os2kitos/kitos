using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class SystemTypeController : GenericOptionApiController<SystemType, ItSystem>
    {
        public SystemTypeController(IGenericRepository<SystemType> repository) 
            : base(repository)
        {
        }
    }
}