using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfaceUsesEntityController : BaseController<ItInterfaceUse>
    {
        public ItInterfaceUsesEntityController(IGenericRepository<ItInterfaceUse> repository)
            : base(repository)
        {
        }
    }
}
