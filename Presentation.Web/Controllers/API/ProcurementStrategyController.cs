using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class ProcurementStrategyController : GenericOptionApiController<ProcurementStrategy, ItContract, OptionDTO>
    {
        public ProcurementStrategyController(IGenericRepository<ProcurementStrategy> repository) 
            : base(repository)
        {
        }
    }
}
