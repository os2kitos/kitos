using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class ProcurementStrategyController : GenericOptionApiController<ProcurementStrategy, ItContract, OptionDTO>
    {
        public ProcurementStrategyController(IGenericRepository<ProcurementStrategy> repository) 
            : base(repository)
        {
        }
    }
}