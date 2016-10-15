using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    public class LocalProcurementStrategyTypesController : LocalOptionBaseController<LocalProcurementStrategyType, ItContract, ProcurementStrategyType>
    {
        public LocalProcurementStrategyTypesController(IGenericRepository<LocalProcurementStrategyType> repository, IAuthenticationService authService, IGenericRepository<ProcurementStrategyType> optionsRepository)
            : base(repository, authService, optionsRepository)
        {
        }
    }
}
