using Core.DomainModel;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class OrganizationRoleController : GenericOptionApiController<OrganizationRole, OrganizationRight, RoleDTO>
    {
        public OrganizationRoleController(IGenericRepository<OrganizationRole> repository) 
            : base(repository)
        {
        }
    }
}
