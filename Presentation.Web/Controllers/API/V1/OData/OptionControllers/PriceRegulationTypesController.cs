using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.API.V1.OData.OptionControllers
{
    [InternalApi]
    public class PriceRegulationTypesController : BaseOptionController<PriceRegulationType, ItContract>
    {
        public PriceRegulationTypesController(IGenericRepository<PriceRegulationType> repository)
            : base(repository)
        {
        }
    }
}