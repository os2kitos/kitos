using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Models;

namespace Presentation.Web.Controllers.API
{
    public class PriceRegulationController : GenericOptionApiController<PriceRegulationType, ItContract, OptionDTO>
    {
        public PriceRegulationController(IGenericRepository<PriceRegulationType> repository)
            : base(repository)
        {
        }
    }
}
