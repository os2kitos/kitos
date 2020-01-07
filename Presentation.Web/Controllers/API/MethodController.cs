using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class MethodController : GenericOptionApiController<MethodType, ItInterface, OptionDTO>
    {
        public MethodController(IGenericRepository<MethodType> repository)
            : base(repository)
        {
        }
    }
}
