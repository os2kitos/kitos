using Core.DomainModel.ItProject;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ItProjectRoleController : GenericOptionApiController<ItProjectRole, ItProjectRight, RoleDTO>
    {
        public ItProjectRoleController(IGenericRepository<ItProjectRole> repository) 
            : base(repository)
        {
        }
    }
}
