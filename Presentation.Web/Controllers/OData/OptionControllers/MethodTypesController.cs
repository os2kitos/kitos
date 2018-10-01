using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MethodTypesController : BaseOptionController<MethodType, ItInterface>
    {
        public MethodTypesController(IGenericRepository<MethodType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}