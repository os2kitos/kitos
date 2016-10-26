using Core.DomainServices;
using Core.DomainModel.Organization;
using System.Web.OData.Routing;
using System.Web.Http;
using System.Web.OData;
using Core.ApplicationServices;
using System.Linq;
namespace Presentation.Web.Controllers.OData
{
    public class OrganizationUnitRolesController : BaseRoleController<OrganizationUnitRole, OrganizationUnitRight>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
