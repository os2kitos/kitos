using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ItInterfaceExhibitsController : BaseEntityController<ItInterfaceExhibit>
    {
        public ItInterfaceExhibitsController(IGenericRepository<ItInterfaceExhibit> repository)
            : base(repository)
        {
        }
    }
}
