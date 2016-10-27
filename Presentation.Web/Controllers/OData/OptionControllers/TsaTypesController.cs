using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using System.Linq;
using System.Web.Http;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class TsaTypesController : BaseRoleController<TsaType, ItInterface>
    {
        public TsaTypesController(IGenericRepository<TsaType> repository, IAuthenticationService authService)
            : base(repository, authService)
        { }
    }
}