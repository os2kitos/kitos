using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfacesController : BaseController<ItInterface>
    {
        public ItInterfacesController(IGenericRepository<ItInterface> repository)
            : base(repository)
        {
        }
    }
}
