using Core.ApplicationServices;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class GlobalConfigsController : BaseEntityController<Core.DomainModel.GlobalConfig>
    {
        public GlobalConfigsController(IGenericRepository<Core.DomainModel.GlobalConfig> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
