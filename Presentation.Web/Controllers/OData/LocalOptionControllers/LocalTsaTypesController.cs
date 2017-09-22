using Core.ApplicationServices;
using Core.DomainModel.ItSystem;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalTsaTypesController : LocalOptionBaseController<LocalTsaType, ItInterface, TsaType>
    {
        public LocalTsaTypesController(IGenericRepository<LocalTsaType> repository, IAuthenticationService authService, IGenericRepository<TsaType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
