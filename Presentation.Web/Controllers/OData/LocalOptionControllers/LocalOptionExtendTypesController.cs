using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalOptionExtendTypesController : LocalOptionBaseController<LocalOptionExtendType, ItContract, OptionExtendType>
    {
        public LocalOptionExtendTypesController(IGenericRepository<LocalOptionExtendType> repository, IAuthenticationService authService, IGenericRepository<OptionExtendType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
