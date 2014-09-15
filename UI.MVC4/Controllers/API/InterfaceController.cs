using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class InterfaceController : GenericOptionApiController<Interface, ItInterface, OptionDTO>
    {
        public InterfaceController(IGenericRepository<Interface> repository) : base(repository)
        {
        }
    }
}
