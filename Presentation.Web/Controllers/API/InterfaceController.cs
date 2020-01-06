using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class InterfaceController : GenericOptionApiController<InterfaceType, ItInterface, OptionDTO>
    {
        public InterfaceController(IGenericRepository<InterfaceType> repository) : base(repository)
        {
        }
    }
}
