using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class OrganizationUnitRoleController : GenericOptionApiController<OrganizationUnitRole, OrganizationUnitRight, RoleDTO>
    {
        public OrganizationUnitRoleController(IGenericRepository<OrganizationUnitRole> repository)
            : base(repository)
        {
        }
    }
}
