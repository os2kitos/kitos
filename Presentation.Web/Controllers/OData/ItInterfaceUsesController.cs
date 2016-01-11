using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfaceUsesController : BaseController<ItInterfaceUse>
    {
        public ItInterfaceUsesController(IGenericRepository<ItInterfaceUse> repository)
            : base(repository)
        {
        }
    }
}
