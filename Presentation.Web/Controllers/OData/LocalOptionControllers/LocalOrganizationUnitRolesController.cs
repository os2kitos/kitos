using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalOrganizationUnitRolesController : LocalOptionBaseController<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole>
    {
        public LocalOrganizationUnitRolesController(IGenericRepository<LocalOrganizationUnitRole> repository, IAuthenticationService authService, IGenericRepository<OrganizationUnitRole> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}