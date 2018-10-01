using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OptionExtendTypesController : BaseOptionController<OptionExtendType, ItContract>
    {
        public OptionExtendTypesController(IGenericRepository<OptionExtendType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}