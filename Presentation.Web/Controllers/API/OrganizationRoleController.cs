using Core.DomainModel;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OrganizationRoleController : GenericOptionApiController<OrganizationRole, OrganizationRight, RoleDTO>
    {
        public OrganizationRoleController(IGenericRepository<OrganizationRole> repository) 
            : base(repository)
        {
        }
    }
}
