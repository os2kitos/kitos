using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TsaTypesController : BaseOptionController<TsaType, ItInterface>
    {
        public TsaTypesController(IGenericRepository<TsaType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}