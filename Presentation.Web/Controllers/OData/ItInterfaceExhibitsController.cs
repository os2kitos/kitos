using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfaceExhibitsController : BaseController<ItInterfaceExhibit>
    {
        public ItInterfaceExhibitsController(IGenericRepository<ItInterfaceExhibit> repository)
            : base(repository)
        {
        }
    }
}
