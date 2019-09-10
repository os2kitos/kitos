using Core.ApplicationServices;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class OrganizationUnitRolesController : BaseOptionController<OrganizationUnitRole, OrganizationUnitRight>
    {
        public OrganizationUnitRolesController(IGenericRepository<OrganizationUnitRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
