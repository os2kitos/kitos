using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ProcurementStrategyController : GenericOptionApiController<ProcurementStrategyType, ItContract, OptionDTO>
    {
        public ProcurementStrategyController(IGenericRepository<ProcurementStrategyType> repository)
            : base(repository)
        {
        }
    }
}
