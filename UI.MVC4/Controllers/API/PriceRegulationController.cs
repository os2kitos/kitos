using Core.DomainModel.ItContract;
using Core.DomainServices;
using UI.MVC4.Models;

namespace UI.MVC4.Controllers.API
{
    public class PriceRegulationController : GenericOptionApiController<PriceRegulation, ItContract, OptionDTO>
    {
        public PriceRegulationController(IGenericRepository<PriceRegulation> repository) 
            : base(repository)
        {
        }
    }
}