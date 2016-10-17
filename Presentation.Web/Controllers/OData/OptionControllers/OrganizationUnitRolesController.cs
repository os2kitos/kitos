using Core.DomainServices;
using Core.DomainModel.Organization;

namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitRolesController : BaseController<OrganizationUnitRole>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository)
            : base(repository)
        {
        }
    }
}
