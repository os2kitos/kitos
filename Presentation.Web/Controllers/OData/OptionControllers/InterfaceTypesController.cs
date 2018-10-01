using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class InterfaceTypesController : BaseOptionController<InterfaceType, ItInterface>
    {
        public InterfaceTypesController(IGenericRepository<InterfaceType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}