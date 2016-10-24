using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class TsaTypesController : BaseEntityController<TsaType>
    {
        public TsaTypesController(IGenericRepository<TsaType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}