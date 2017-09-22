using Core.ApplicationServices;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class ConfigsController : BaseEntityController<Core.DomainModel.Config>
    {
        public ConfigsController(IGenericRepository<Core.DomainModel.Config> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
