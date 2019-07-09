using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalItProjectRolesController : LocalOptionBaseController<LocalItProjectRole, ItProjectRight, ItProjectRole>
    {
        public LocalItProjectRolesController(IGenericRepository<LocalItProjectRole> repository, IAuthenticationService authService, IGenericRepository<ItProjectRole> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
