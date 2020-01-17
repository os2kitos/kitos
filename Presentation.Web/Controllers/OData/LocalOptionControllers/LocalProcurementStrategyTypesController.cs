using Core.DomainModel.ItContract;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.LocalOptionControllers
{
    [InternalApi]
    public class LocalProcurementStrategyTypesController : LocalOptionBaseController<LocalProcurementStrategyType, ItContract, ProcurementStrategyType>
    {
        public LocalProcurementStrategyTypesController(IGenericRepository<LocalProcurementStrategyType> repository, IGenericRepository<ProcurementStrategyType> optionsRepository)
            : base(repository, optionsRepository)
        {
        }
    }
}
