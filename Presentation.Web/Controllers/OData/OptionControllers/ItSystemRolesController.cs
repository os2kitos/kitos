using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ItSystemRolesController : BaseRoleController<ItSystemRole, ItSystemRight>
    {
        public ItSystemRolesController(IGenericRepository<ItSystemRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
