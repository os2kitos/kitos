using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class PriceRegulationController : GenericOptionApiController<PriceRegulation, ItContract, OptionDTO>
    {
        public PriceRegulationController(IGenericRepository<PriceRegulation> repository) 
            : base(repository)
        {
        }
    }
}
