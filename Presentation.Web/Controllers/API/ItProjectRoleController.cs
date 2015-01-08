using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ItProjectRoleController : GenericOptionApiController<ItProjectRole, ItProjectRight, RoleDTO>
    {
        public ItProjectRoleController(IGenericRepository<ItProjectRole> repository) 
            : base(repository)
        {
        }
    }
}
