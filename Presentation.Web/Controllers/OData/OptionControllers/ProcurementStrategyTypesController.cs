using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProcurementStrategyTypesController : BaseOptionController<ProcurementStrategyType, ItContract>
    {
        public ProcurementStrategyTypesController(IGenericRepository<ProcurementStrategyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}