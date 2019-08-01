using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    [PublicApi]
    public class ProcurementStrategyController : GenericOptionApiController<ProcurementStrategyType, ItContract, OptionDTO>
    {
        public ProcurementStrategyController(IGenericRepository<ProcurementStrategyType> repository)
            : base(repository)
        {
        }
    }
}
