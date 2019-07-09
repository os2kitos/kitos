using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItProjectRolesController : BaseOptionController<ItProjectRole, ItProjectRight>
    {
        public ItProjectRolesController(IGenericRepository<ItProjectRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
