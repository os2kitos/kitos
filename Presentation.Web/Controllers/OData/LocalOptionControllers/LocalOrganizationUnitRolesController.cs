using Core.ApplicationServices;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    [ControllerEvaluationCompleted]
    public class LocalOrganizationUnitRolesController : LocalOptionBaseController<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole>
    {
        public LocalOrganizationUnitRolesController(IGenericRepository<LocalOrganizationUnitRole> repository, IAuthenticationService authService, IGenericRepository<OrganizationUnitRole> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}