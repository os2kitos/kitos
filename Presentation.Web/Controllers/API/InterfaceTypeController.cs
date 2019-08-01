using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class InterfaceTypeController : GenericOptionApiController<ItInterfaceType, ItInterface, OptionDTO>
    {
        public InterfaceTypeController(IGenericRepository<ItInterfaceType> repository) 
            : base(repository)
        {
        }
    }
}
