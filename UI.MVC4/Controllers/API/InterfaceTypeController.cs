using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace UI.MVC4.Controllers
{
    public class InterfaceTypeController : GenericApiController<InterfaceType, int>
    {
        public InterfaceTypeController(IGenericRepository<InterfaceType> repository) 
            : base(repository)
        {
        }
    }
}