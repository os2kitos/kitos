using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalItInterfaceTypesController : LocalOptionBaseController<LocalItInterfaceType, ItInterface, ItInterfaceType>
    {
        public LocalItInterfaceTypesController(IGenericRepository<LocalItInterfaceType> repository, IAuthenticationService authService, IGenericRepository<ItInterfaceType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
