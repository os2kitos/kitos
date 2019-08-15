using Core.ApplicationServices;
using Core.DomainModel.ItProject;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ItProjectRolesController : BaseOptionController<ItProjectRole, ItProjectRight>
    {
        public ItProjectRolesController(IGenericRepository<ItProjectRole> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
