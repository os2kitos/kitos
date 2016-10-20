using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class OrganizationUnitRolesController : BaseEntityController<OrganizationUnitRole>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
