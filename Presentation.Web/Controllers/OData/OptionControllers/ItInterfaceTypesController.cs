using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ItInterfaceTypesController : BaseOptionController<ItInterfaceType, ItInterface>
    {
        public ItInterfaceTypesController(IGenericRepository<ItInterfaceType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}