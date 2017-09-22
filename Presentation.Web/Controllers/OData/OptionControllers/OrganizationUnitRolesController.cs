using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class OrganizationUnitRolesController : BaseOptionController<OrganizationUnitRole, OrganizationUnitRight>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
