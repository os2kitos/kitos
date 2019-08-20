using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Access;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemTypeOptionController : GenericOptionApiController<ItSystemType, ItSystem, OptionDTO>
    {
        public ItSystemTypeOptionController(IGenericRepository<ItSystemType> repository, IAccessContext accessContext)
            : base(repository, accessContext)
        {
        }
    }
}
