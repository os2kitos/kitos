using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class InterfacesController : BaseController<Interface>
    {
        public InterfacesController(IGenericRepository<Interface> repository)
            : base(repository)
        {
        }
    }
}
