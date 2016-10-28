using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ItInterfaceTypesController : BaseOptionController<ItInterfaceType, ItInterface>
    {
        public ItInterfaceTypesController(IGenericRepository<ItInterfaceType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}