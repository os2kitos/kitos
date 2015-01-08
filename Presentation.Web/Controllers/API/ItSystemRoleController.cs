using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItSystemRoleController : GenericOptionApiController<ItSystemRole, ItSystemRight, RoleDTO>
    {
        public ItSystemRoleController(IGenericRepository<ItSystemRole> repository) 
            : base(repository)
        {
        }
    }
}
