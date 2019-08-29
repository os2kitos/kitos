using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Infrastructure.Authorization.Context;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ItSystemRoleController : GenericOptionApiController<ItSystemRole, ItSystemRight, RoleDTO>
    {
        public ItSystemRoleController(IGenericRepository<ItSystemRole> repository, IAuthorizationContext authorizationContext) 
            : base(repository, authorizationContext)
        {
        }
    }
}
