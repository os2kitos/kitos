using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalTsaTypesController : LocalOptionBaseController<LocalTsaType, ItInterface, TsaType>
    {
        public LocalTsaTypesController(IGenericRepository<LocalTsaType> repository, IAuthenticationService authService, IGenericRepository<TsaType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
