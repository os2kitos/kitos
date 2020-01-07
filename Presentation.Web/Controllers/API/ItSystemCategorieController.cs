using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class ItSystemCategorieController : GenericOptionApiController<ItSystemCategories, ItSystemUsage, OptionDTO>
    {
        ItSystemCategorieController(IGenericRepository<ItSystemCategories> repository, IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext) { }
    }
}