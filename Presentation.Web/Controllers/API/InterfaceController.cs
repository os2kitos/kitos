using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class InterfaceController : GenericOptionApiController<Interface, ItInterface, OptionDTO>
    {
        public InterfaceController(IGenericRepository<Interface> repository) : base(repository)
        {
        }
    }
}
