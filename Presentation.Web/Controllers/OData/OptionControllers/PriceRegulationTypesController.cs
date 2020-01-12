using Core.DomainModel.ItContract;
using Core.DomainServices;
using Presentation.Web.Infrastructure.Attributes;

namespace Presentation.Web.Controllers.OData.OptionControllers
{
    [InternalApi]
    [MigratedToNewAuthorizationContext]
    public class PriceRegulationTypesController : BaseOptionController<PriceRegulationType, ItContract>
    {
        public PriceRegulationTypesController(IGenericRepository<PriceRegulationType> repository)
            : base(repository)
        {
        }
    }
}