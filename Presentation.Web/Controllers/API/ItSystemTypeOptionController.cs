using Core.ApplicationServices.Authorization;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    [DeprecatedApi]
    public class ItSystemTypeOptionController : GenericOptionApiController<ItSystemType, ItSystem, OptionDTO>
    {
        public ItSystemTypeOptionController(IGenericRepository<ItSystemType> repository, IAuthorizationContext authorizationContext)
            : base(repository, authorizationContext)
        {
        }
    }
}
