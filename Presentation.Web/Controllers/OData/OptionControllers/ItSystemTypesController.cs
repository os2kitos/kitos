using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ItSystemTypesController : BaseOptionController<ItSystemType, ItSystem>
    {
        public ItSystemTypesController(IGenericRepository<ItSystemType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}