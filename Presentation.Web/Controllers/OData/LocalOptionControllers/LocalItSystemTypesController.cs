using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalItSystemTypesController : LocalOptionBaseController<LocalItSystemType, ItSystem, ItSystemType>
    {
        public LocalItSystemTypesController(IGenericRepository<LocalItSystemType> repository, IAuthenticationService authService, IGenericRepository<ItSystemType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
