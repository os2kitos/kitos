using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData
{
    public class InterfaceTypesController : BaseEntityController<InterfaceType>
    {
        public InterfaceTypesController(IGenericRepository<InterfaceType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}
