using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalMethodTypesController : LocalOptionBaseController<LocalMethodType, ItInterface, MethodType>
    {
        public LocalMethodTypesController(IGenericRepository<LocalMethodType> repository, IAuthenticationService authService, IGenericRepository<MethodType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
