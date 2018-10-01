using System.Web.Http.Description;
using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class LocalProcurementStrategyTypesController : LocalOptionBaseController<LocalProcurementStrategyType, ItContract, ProcurementStrategyType>
    {
        public LocalProcurementStrategyTypesController(IGenericRepository<LocalProcurementStrategyType> repository, IAuthenticationService authService, IGenericRepository<ProcurementStrategyType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
