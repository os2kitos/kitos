using Core.ApplicationServices;
using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    public class ProcurementStrategyTypesController : BaseOptionController<ProcurementStrategyType, ItContract>
    {
        public ProcurementStrategyTypesController(IGenericRepository<ProcurementStrategyType> repository, IAuthenticationService authService)
            : base(repository, authService)
        {
        }
    }
}