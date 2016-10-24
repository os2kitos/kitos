using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    public class ProcurementStrategyTypesController : BaseEntityController<ProcurementStrategyType>
    {
        public ProcurementStrategyTypesController(IGenericRepository<ProcurementStrategyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}