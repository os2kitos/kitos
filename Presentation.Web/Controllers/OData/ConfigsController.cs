using Core.ApplicationServices;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData
{
    [InternalApi]
    public class ConfigsController : BaseEntityController<Core.DomainModel.Config>
    {
        public ConfigsController(IGenericRepository<Core.DomainModel.Config> repository, IAuthenticationService authService)
            : base(repository)
        {
        }
    }
}
