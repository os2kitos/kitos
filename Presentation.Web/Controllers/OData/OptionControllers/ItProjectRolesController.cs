using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ItProjectRolesController : BaseEntityController<ItProjectRole>
    {
        public ItProjectRolesController(IGenericRepository<ItProjectRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
