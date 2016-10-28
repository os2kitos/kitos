using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class InterfaceTypesController : BaseOptionController<InterfaceType, ItInterface>
    {
        public InterfaceTypesController(IGenericRepository<InterfaceType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}