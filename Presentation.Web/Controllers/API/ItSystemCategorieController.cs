using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Access;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemCategorieController : GenericOptionApiController<ItSystemCategories, ItSystemUsage, OptionDTO>
    {
        ItSystemCategorieController(IGenericRepository<ItSystemCategories> repository, IAccessContext accessContext)
            : base(repository, accessContext) { }
    }
}