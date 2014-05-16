using Core.DomainModel.ItSystem;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItSystemRoleController : GenericOptionApiController<ItSystemRole, ItSystemRight, RoleDTO>
    {
        public ItSystemRoleController(IGenericRepository<ItSystemRole> repository) 
            : base(repository)
        {
        }
    }
}
