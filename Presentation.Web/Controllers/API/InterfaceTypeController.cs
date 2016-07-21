using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class InterfaceTypeController : GenericOptionApiController<ItInterfaceType, ItInterface, OptionDTO>
    {
        public InterfaceTypeController(IGenericRepository<ItInterfaceType> repository) 
            : base(repository)
        {
        }
    }
}
